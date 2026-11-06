using UnityEngine;
using UnityEngine.UI;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// Main menu screen with Play button, High Score display, sound toggle, and debug seed.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Text soundButtonText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private InputField seedInput;

        private void Awake()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }

            if (soundButton != null)
            {
                soundButton.onClick.AddListener(OnSoundToggleClicked);
            }
        }

        public void Show(int bestScore)
        {
            gameObject.SetActive(true);
            if (bestScoreText != null)
            {
                bestScoreText.text = $"BEST DISTANCE: {bestScore}m";
            }
            RefreshSoundText();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnPlayClicked()
        {
            SoundManager.Instance?.PlaySound(SoundEffectType.ButtonClick);
            Hide();
            UIManager.Instance?.StartGame();
        }

        private void OnSoundToggleClicked()
        {
            SoundManager.Instance?.ToggleSound();
            RefreshSoundText();
            SoundManager.Instance?.PlaySound(SoundEffectType.ButtonClick);
        }

        private void RefreshSoundText()
        {
            if (soundButtonText != null)
            {
                bool soundOn = SoundManager.Instance != null && SoundManager.Instance.IsSoundEnabled;
                soundButtonText.text = soundOn ? "SOUND: ON" : "SOUND: OFF";
            }
        }
    }
}
