using UnityEngine;
using UnityEngine.UI;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// Game over screen shown when crowd count hits 0.
    /// Displays final distance score, best score, new record celebration, and Retry button.
    /// </summary>
    public class GameOverView : MonoBehaviour
    {
        [SerializeField] private Text finalDistanceText;
        [SerializeField] private Text bestDistanceText;
        [SerializeField] private GameObject newRecordBadge;
        [SerializeField] private Button retryButton;

        private void Awake()
        {
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }
        }

        public void Show(int finalDistance, int bestDistance, bool isNewRecord)
        {
            gameObject.SetActive(true);

            if (finalDistanceText != null)
            {
                finalDistanceText.text = $"DISTANCE: {finalDistance}m";
            }

            if (bestDistanceText != null)
            {
                bestDistanceText.text = $"BEST: {bestDistance}m";
            }

            if (newRecordBadge != null)
            {
                newRecordBadge.SetActive(isNewRecord);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnRetryClicked()
        {
            SoundManager.Instance?.PlaySound(SoundEffectType.ButtonClick);
            Hide();
            UIManager.Instance?.RestartGame();
        }
    }
}
