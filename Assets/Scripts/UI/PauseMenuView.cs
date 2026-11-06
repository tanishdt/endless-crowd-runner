using UnityEngine;
using UnityEngine.UI;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// Pause overlay allowing player to resume, restart, or toggle audio.
    /// </summary>
    public class PauseMenuView : MonoBehaviour
    {
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button soundButton;
        [SerializeField] private Text soundButtonText;

        private void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (soundButton != null) soundButton.onClick.AddListener(OnSoundClicked);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Time.timeScale = 0f;
            RefreshSoundText();
        }

        public void Hide()
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        private void OnResumeClicked()
        {
            SoundManager.Instance?.PlaySound(SoundEffectType.ButtonClick);
            Hide();
        }

        private void OnRestartClicked()
        {
            Time.timeScale = 1f;
            SoundManager.Instance?.PlaySound(SoundEffectType.ButtonClick);
            Hide();
            UIManager.Instance?.RestartGame();
        }

        private void OnSoundClicked()
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
