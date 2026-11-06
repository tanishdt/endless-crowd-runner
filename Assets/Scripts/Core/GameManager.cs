using System;
using UnityEngine;
using EndlessCrowdRunner.Difficulty;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Environment;
using EndlessCrowdRunner.Camera;
using EndlessCrowdRunner.UI;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.Core
{
    /// <summary>
    /// Master game controller managing game states, high score persistence,
    /// run initialization, resets, and coordination between all core systems.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private const string PrefBestScore = "ECR_BestScore";

        [Header("Configuration")]
        [SerializeField] private GameConfig config;

        [Header("Scene References")]
        [SerializeField] private CrowdController crowdController;
        [SerializeField] private ChunkGenerator chunkGenerator;
        [SerializeField] private DifficultyScaler difficultyScaler;
        [SerializeField] private CameraFollow cameraFollow;

        public GameState State { get; private set; } = GameState.MainMenu;
        public int BestScore { get; private set; } = 0;

        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            LoadBestScore();
            EnsureDefaultConfig();
        }

        private void EnsureDefaultConfig()
        {
            if (config == null)
            {
                config = Resources.Load<GameConfig>("GameConfig");
                if (config == null)
                {
                    config = ScriptableObject.CreateInstance<GameConfig>();
                }
            }
        }

        private void Start()
        {
            // Find references if not wired in Inspector
            if (crowdController == null) crowdController = FindObjectOfType<CrowdController>();
            if (chunkGenerator == null) chunkGenerator = FindObjectOfType<ChunkGenerator>();
            if (difficultyScaler == null) difficultyScaler = FindObjectOfType<DifficultyScaler>();
            if (cameraFollow == null) cameraFollow = FindObjectOfType<CameraFollow>();

            // Hook crowd death event
            if (crowdController != null)
            {
                crowdController.OnCrowdEliminated += HandleCrowdEliminated;
            }

            // Enter Main Menu state
            SetState(GameState.MainMenu);
            UIManager.Instance?.ShowMainMenu(BestScore);
        }

        public void StartRun()
        {
            Time.timeScale = 1f;

            if (difficultyScaler != null)
            {
                difficultyScaler.Initialize(config, crowdController);
            }

            if (crowdController != null)
            {
                crowdController.Initialize(config);
            }

            if (chunkGenerator != null)
            {
                chunkGenerator.Initialize(config, crowdController, difficultyScaler);
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(crowdController);
            }

            SetState(GameState.Playing);

            UIManager.Instance?.ShowHUD();
            UIManager.Instance?.BindGameplay(crowdController, difficultyScaler, BestScore);
        }

        public void RestartRun()
        {
            StartRun();
        }

        private void HandleCrowdEliminated()
        {
            if (State != GameState.Playing) return;

            int finalDistance = difficultyScaler != null ? Mathf.RoundToInt(difficultyScaler.DistanceTraveled) : 0;
            bool isNewRecord = false;

            if (finalDistance > BestScore)
            {
                BestScore = finalDistance;
                SaveBestScore(BestScore);
                isNewRecord = true;
            }

            SetState(GameState.GameOver);
            UIManager.Instance?.ShowGameOver(finalDistance, BestScore, isNewRecord);
        }

        public void PauseGame()
        {
            if (State == GameState.Playing)
            {
                SetState(GameState.Paused);
                UIManager.Instance?.OpenPauseMenu();
            }
        }

        public void ResumeGame()
        {
            if (State == GameState.Paused)
            {
                Time.timeScale = 1f;
                SetState(GameState.Playing);
            }
        }

        private void SetState(GameState newState)
        {
            State = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void LoadBestScore()
        {
            BestScore = PlayerPrefs.GetInt(PrefBestScore, 0);
        }

        private void SaveBestScore(int score)
        {
            PlayerPrefs.SetInt(PrefBestScore, score);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (crowdController != null)
            {
                crowdController.OnCrowdEliminated -= HandleCrowdEliminated;
            }
        }
    }
}
