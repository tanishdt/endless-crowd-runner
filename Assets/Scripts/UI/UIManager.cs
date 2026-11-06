using UnityEngine;
using UnityEngine.UI;
using EndlessCrowdRunner.Core;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Difficulty;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// Master UI manager controlling transitions between MainMenu, HUD, PauseMenu, and GameOver.
    /// Can also build procedural Canvas elements if run in a clean scene.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Views")]
        [SerializeField] private HUDView hudView;
        [SerializeField] private MainMenuView mainMenuView;
        [SerializeField] private PauseMenuView pauseMenuView;
        [SerializeField] private GameOverView gameOverView;
        [SerializeField] private DebugFPSOverlay debugOverlay;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureUIHierarchy();
        }

        private void EnsureUIHierarchy()
        {
            if (debugOverlay == null)
            {
                debugOverlay = gameObject.AddComponent<DebugFPSOverlay>();
            }

            // Find or construct Canvas if missing
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                canvasGo.AddComponent<GraphicRaycaster>();
            }

            // Ensure EventSystem
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Setup View references or procedural fallbacks
            if (hudView == null) hudView = canvas.GetComponentInChildren<HUDView>(true) ?? CreateProceduralHUD(canvas.transform);
            if (mainMenuView == null) mainMenuView = canvas.GetComponentInChildren<MainMenuView>(true) ?? CreateProceduralMainMenu(canvas.transform);
            if (pauseMenuView == null) pauseMenuView = canvas.GetComponentInChildren<PauseMenuView>(true) ?? CreateProceduralPauseMenu(canvas.transform);
            if (gameOverView == null) gameOverView = canvas.GetComponentInChildren<GameOverView>(true) ?? CreateProceduralGameOver(canvas.transform);
        }

        public void BindGameplay(CrowdController crowd, DifficultyScaler scaler, int bestScore)
        {
            if (hudView != null)
            {
                hudView.Bind(crowd, scaler, bestScore);
            }
        }

        public void ShowMainMenu(int bestScore)
        {
            if (hudView != null) hudView.gameObject.SetActive(false);
            if (pauseMenuView != null) pauseMenuView.Hide();
            if (gameOverView != null) gameOverView.Hide();
            if (mainMenuView != null) mainMenuView.Show(bestScore);
        }

        public void ShowHUD()
        {
            if (mainMenuView != null) mainMenuView.Hide();
            if (pauseMenuView != null) pauseMenuView.Hide();
            if (gameOverView != null) gameOverView.Hide();
            if (hudView != null) hudView.gameObject.SetActive(true);
        }

        public void OpenPauseMenu()
        {
            if (pauseMenuView != null) pauseMenuView.Show();
        }

        public void ShowGameOver(int finalDistance, int bestDistance, bool isNewRecord)
        {
            if (hudView != null) hudView.gameObject.SetActive(false);
            if (pauseMenuView != null) pauseMenuView.Hide();
            if (gameOverView != null) gameOverView.Show(finalDistance, bestDistance, isNewRecord);
        }

        public void StartGame()
        {
            GameManager.Instance?.StartRun();
        }

        public void RestartGame()
        {
            GameManager.Instance?.RestartRun();
        }

        #region Procedural View Fallbacks

        private HUDView CreateProceduralHUD(Transform parent)
        {
            GameObject hudGo = new GameObject("HUDView");
            hudGo.transform.SetParent(parent, false);
            RectTransform rt = hudGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Distance score top-left
            GameObject distGo = CreateTextObject(hudGo.transform, "DistanceText", "0m", 48, TextAnchor.MiddleLeft);
            RectTransform distRt = distGo.GetComponent<RectTransform>();
            distRt.anchorMin = new Vector2(0f, 1f);
            distRt.anchorMax = new Vector2(0f, 1f);
            distRt.anchoredPosition = new Vector2(250f, -120f);
            distRt.sizeDelta = new Vector2(400f, 80f);

            // Best score top-left secondary
            GameObject bestGo = CreateTextObject(hudGo.transform, "BestScoreText", "BEST: 0m", 32, TextAnchor.MiddleLeft);
            RectTransform bestRt = bestGo.GetComponent<RectTransform>();
            bestRt.anchorMin = new Vector2(0f, 1f);
            bestRt.anchorMax = new Vector2(0f, 1f);
            bestRt.anchoredPosition = new Vector2(250f, -180f);
            bestRt.sizeDelta = new Vector2(400f, 60f);

            // Pause button top-right
            GameObject pauseBtnGo = CreateButtonObject(hudGo.transform, "PauseButton", "||", new Vector2(Screen.width > 0 ? 1000f : 960f, -120f), new Vector2(100f, 100f));
            RectTransform pauseRt = pauseBtnGo.GetComponent<RectTransform>();
            pauseRt.anchorMin = new Vector2(1f, 1f);
            pauseRt.anchorMax = new Vector2(1f, 1f);
            pauseRt.anchoredPosition = new Vector2(-120f, -120f);

            // Current weapon badge
            GameObject wpnGo = CreateTextObject(hudGo.transform, "WeaponText", "", 36, TextAnchor.UpperCenter);
            RectTransform wpnRt = wpnGo.GetComponent<RectTransform>();
            wpnRt.anchorMin = new Vector2(0.5f, 1f);
            wpnRt.anchorMax = new Vector2(0.5f, 1f);
            wpnRt.anchoredPosition = new Vector2(0f, -220f);
            wpnRt.sizeDelta = new Vector2(400f, 60f);

            HUDView view = hudGo.AddComponent<HUDView>();
            SetPrivateField(view, "distanceText", distGo.GetComponent<Text>());
            SetPrivateField(view, "bestScoreText", bestGo.GetComponent<Text>());
            SetPrivateField(view, "weaponText", wpnGo.GetComponent<Text>());
            SetPrivateField(view, "pauseButton", pauseBtnGo.GetComponent<Button>());

            return view;
        }

        private MainMenuView CreateProceduralMainMenu(Transform parent)
        {
            GameObject menuGo = new GameObject("MainMenuView");
            menuGo.transform.SetParent(parent, false);
            RectTransform rt = menuGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Semi-transparent background
            Image bg = menuGo.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.09f, 0.12f, 0.88f);

            // Title
            GameObject titleGo = CreateTextObject(menuGo.transform, "TitleText", "CROWD RUNNER\n3D", 76, TextAnchor.MiddleCenter);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.7f);
            titleRt.anchorMax = new Vector2(0.5f, 0.7f);
            titleRt.anchoredPosition = Vector2.zero;
            titleRt.sizeDelta = new Vector2(800f, 220f);

            // Best score
            GameObject bestGo = CreateTextObject(menuGo.transform, "BestScoreText", "BEST DISTANCE: 0m", 40, TextAnchor.MiddleCenter);
            RectTransform bestRt = bestGo.GetComponent<RectTransform>();
            bestRt.anchorMin = new Vector2(0.5f, 0.52f);
            bestRt.anchorMax = new Vector2(0.5f, 0.52f);
            bestRt.anchoredPosition = Vector2.zero;
            bestRt.sizeDelta = new Vector2(600f, 80f);

            // Play button
            GameObject playBtnGo = CreateButtonObject(menuGo.transform, "PlayButton", "TAP TO RUN", new Vector2(0f, -120f), new Vector2(440f, 130f), new Color(0.15f, 0.8f, 0.35f));
            RectTransform playRt = playBtnGo.GetComponent<RectTransform>();
            playRt.anchorMin = new Vector2(0.5f, 0.38f);
            playRt.anchorMax = new Vector2(0.5f, 0.38f);
            playRt.anchoredPosition = Vector2.zero;

            // Sound toggle button
            GameObject sndBtnGo = CreateButtonObject(menuGo.transform, "SoundButton", "SOUND: ON", new Vector2(0f, -320f), new Vector2(280f, 80f), new Color(0.3f, 0.35f, 0.45f));
            RectTransform sndRt = sndBtnGo.GetComponent<RectTransform>();
            sndRt.anchorMin = new Vector2(0.5f, 0.22f);
            sndRt.anchorMax = new Vector2(0.5f, 0.22f);
            sndRt.anchoredPosition = Vector2.zero;

            MainMenuView view = menuGo.AddComponent<MainMenuView>();
            SetPrivateField(view, "playButton", playBtnGo.GetComponent<Button>());
            SetPrivateField(view, "soundButton", sndBtnGo.GetComponent<Button>());
            SetPrivateField(view, "soundButtonText", sndBtnGo.GetComponentInChildren<Text>());
            SetPrivateField(view, "bestScoreText", bestGo.GetComponent<Text>());

            return view;
        }

        private PauseMenuView CreateProceduralPauseMenu(Transform parent)
        {
            GameObject pauseGo = new GameObject("PauseMenuView");
            pauseGo.transform.SetParent(parent, false);
            RectTransform rt = pauseGo.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = pauseGo.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.75f);

            GameObject titleGo = CreateTextObject(pauseGo.transform, "PauseTitle", "PAUSED", 68, TextAnchor.MiddleCenter);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.68f);
            titleRt.anchorMax = new Vector2(0.5f, 0.68f);
            titleRt.anchoredPosition = Vector2.zero;
            titleRt.sizeDelta = new Vector2(600f, 100f);

            GameObject resumeBtnGo = CreateButtonObject(pauseGo.transform, "ResumeBtn", "RESUME", Vector2.zero, new Vector2(400f, 100f), new Color(0.2f, 0.75f, 0.35f));
            RectTransform rRt = resumeBtnGo.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.5f, 0.52f);
            rRt.anchorMax = new Vector2(0.5f, 0.52f);
            rRt.anchoredPosition = Vector2.zero;

            GameObject restartBtnGo = CreateButtonObject(pauseGo.transform, "RestartBtn", "RESTART", Vector2.zero, new Vector2(400f, 100f), new Color(0.9f, 0.45f, 0.15f));
            RectTransform resRt = restartBtnGo.GetComponent<RectTransform>();
            resRt.anchorMin = new Vector2(0.5f, 0.40f);
            resRt.anchorMax = new Vector2(0.5f, 0.40f);
            resRt.anchoredPosition = Vector2.zero;

            GameObject soundBtnGo = CreateButtonObject(pauseGo.transform, "SoundBtn", "SOUND: ON", Vector2.zero, new Vector2(400f, 90f), new Color(0.3f, 0.35f, 0.45f));
            RectTransform sRt = soundBtnGo.GetComponent<RectTransform>();
            sRt.anchorMin = new Vector2(0.5f, 0.28f);
            sRt.anchorMax = new Vector2(0.5f, 0.28f);
            sRt.anchoredPosition = Vector2.zero;

            PauseMenuView view = pauseGo.AddComponent<PauseMenuView>();
            SetPrivateField(view, "resumeButton", resumeBtnGo.GetComponent<Button>());
            SetPrivateField(view, "restartButton", restartBtnGo.GetComponent<Button>());
            SetPrivateField(view, "soundButton", soundBtnGo.GetComponent<Button>());
            SetPrivateField(view, "soundButtonText", soundBtnGo.GetComponentInChildren<Text>());

            pauseGo.SetActive(false);
            return view;
        }

        private GameOverView CreateProceduralGameOver(Transform parent)
        {
            GameObject go = new GameObject("GameOverView");
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image bg = go.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.04f, 0.04f, 0.92f);

            GameObject titleGo = CreateTextObject(go.transform, "WipedOutText", "CROWD WIPED OUT!", 64, TextAnchor.MiddleCenter, Color.red);
            RectTransform titleRt = titleGo.GetComponent<RectTransform>();
            titleRt.anchorMin = new Vector2(0.5f, 0.72f);
            titleRt.anchorMax = new Vector2(0.5f, 0.72f);
            titleRt.anchoredPosition = Vector2.zero;
            titleRt.sizeDelta = new Vector2(800f, 100f);

            GameObject distGo = CreateTextObject(go.transform, "FinalDistText", "DISTANCE: 0m", 48, TextAnchor.MiddleCenter);
            RectTransform distRt = distGo.GetComponent<RectTransform>();
            distRt.anchorMin = new Vector2(0.5f, 0.58f);
            distRt.anchorMax = new Vector2(0.5f, 0.58f);
            distRt.anchoredPosition = Vector2.zero;
            distRt.sizeDelta = new Vector2(600f, 80f);

            GameObject bestGo = CreateTextObject(go.transform, "BestDistText", "BEST: 0m", 38, TextAnchor.MiddleCenter, Color.yellow);
            RectTransform bestRt = bestGo.GetComponent<RectTransform>();
            bestRt.anchorMin = new Vector2(0.5f, 0.50f);
            bestRt.anchorMax = new Vector2(0.5f, 0.50f);
            bestRt.anchoredPosition = Vector2.zero;
            bestRt.sizeDelta = new Vector2(600f, 60f);

            GameObject recordBadge = CreateTextObject(go.transform, "RecordBadge", "*** NEW BEST RECORD! ***", 40, TextAnchor.MiddleCenter, Color.yellow);
            RectTransform recRt = recordBadge.GetComponent<RectTransform>();
            recRt.anchorMin = new Vector2(0.5f, 0.42f);
            recRt.anchorMax = new Vector2(0.5f, 0.42f);
            recRt.anchoredPosition = Vector2.zero;
            recRt.sizeDelta = new Vector2(600f, 60f);

            GameObject retryBtnGo = CreateButtonObject(go.transform, "RetryBtn", "PLAY AGAIN", Vector2.zero, new Vector2(440f, 120f), new Color(0.15f, 0.8f, 0.35f));
            RectTransform rRt = retryBtnGo.GetComponent<RectTransform>();
            rRt.anchorMin = new Vector2(0.5f, 0.28f);
            rRt.anchorMax = new Vector2(0.5f, 0.28f);
            rRt.anchoredPosition = Vector2.zero;

            GameOverView view = go.AddComponent<GameOverView>();
            SetPrivateField(view, "finalDistanceText", distGo.GetComponent<Text>());
            SetPrivateField(view, "bestDistanceText", bestGo.GetComponent<Text>());
            SetPrivateField(view, "newRecordBadge", recordBadge);
            SetPrivateField(view, "retryButton", retryBtnGo.GetComponent<Button>());

            go.SetActive(false);
            return view;
        }

        private GameObject CreateTextObject(Transform parent, string name, string content, int fontSize, TextAnchor anchor, Color? color = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            Text text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = color ?? Color.white;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (text.font == null) text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return go;
        }

        private GameObject CreateButtonObject(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color? btnColor = null)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            Image img = go.AddComponent<Image>();
            img.color = btnColor ?? new Color(0.2f, 0.5f, 0.9f);

            Button btn = go.AddComponent<Button>();
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            GameObject textGo = CreateTextObject(go.transform, "Text", label, 38, TextAnchor.MiddleCenter);
            RectTransform textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;

            return go;
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        #endregion
    }
}
