using UnityEngine;
using UnityEngine.UI;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Weapons;
using EndlessCrowdRunner.Difficulty;
using EndlessCrowdRunner.Audio;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// HUD displaying real-time crowd count, distance score, best record,
    /// equipped weapon, and above-crowd 3D count banner.
    /// </summary>
    public class HUDView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text countText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text weaponText;
        [SerializeField] private Button pauseButton;

        [Header("Above Crowd Billboard")]
        [SerializeField] private TextMesh aboveCrowdCountText;
        [SerializeField] private Transform aboveCrowdRoot;

        private CrowdController _crowd;
        private DifficultyScaler _scaler;
        private Vector3 _originalCountScale = Vector3.one;

        private void Awake()
        {
            if (countText != null) _originalCountScale = countText.transform.localScale;

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(() =>
                {
                    SoundManager.Instance?.PlaySound(SoundEffectType.ButtonClick);
                    UIManager.Instance?.OpenPauseMenu();
                });
            }

            // Create above crowd billboard if not present
            if (aboveCrowdRoot == null)
            {
                GameObject root = new GameObject("AboveCrowdCounter");
                aboveCrowdRoot = root.transform;

                GameObject textGo = new GameObject("CountText");
                textGo.transform.SetParent(aboveCrowdRoot);
                textGo.transform.localPosition = Vector3.zero;

                aboveCrowdCountText = textGo.AddComponent<TextMesh>();
                aboveCrowdCountText.fontSize = 54;
                aboveCrowdCountText.characterSize = 0.14f;
                aboveCrowdCountText.alignment = TextAlignment.Center;
                aboveCrowdCountText.anchor = TextAnchor.MiddleCenter;
                aboveCrowdCountText.color = new Color(0.1f, 0.9f, 1.0f);
            }
        }

        public void Bind(CrowdController crowd, DifficultyScaler scaler, int bestScore)
        {
            _crowd = crowd;
            _scaler = scaler;

            if (_crowd != null)
            {
                _crowd.OnCountChanged += UpdateCountDisplay;
                UpdateCountDisplay(_crowd.CurrentCount);

                WeaponSystem ws = _crowd.GetComponent<WeaponSystem>();
                if (ws != null)
                {
                    ws.OnWeaponUpgraded += (type, wName) => UpdateWeaponDisplay(wName);
                    UpdateWeaponDisplay(ws.CurrentData != null ? ws.CurrentData.weaponName : "Unarmed");
                }
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = $"BEST: {bestScore}m";
            }
        }

        public void UpdateCountDisplay(int count)
        {
            if (countText != null)
            {
                countText.text = count.ToString();
                // Punch scale bounce on count change
                StartCoroutine(PunchCountText());
            }

            if (aboveCrowdCountText != null)
            {
                aboveCrowdCountText.text = count.ToString();
            }
        }

        private System.Collections.IEnumerator PunchCountText()
        {
            if (countText == null) yield break;
            countText.transform.localScale = _originalCountScale * 1.35f;

            float elapsed = 0f;
            float duration = 0.18f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                countText.transform.localScale = Vector3.Lerp(countText.transform.localScale, _originalCountScale, elapsed / duration);
                yield return null;
            }

            countText.transform.localScale = _originalCountScale;
        }

        public void UpdateWeaponDisplay(string weaponName)
        {
            if (weaponText != null)
            {
                weaponText.text = weaponName.ToUpper();
            }
        }

        private void LateUpdate()
        {
            // Update distance
            if (_scaler != null && distanceText != null)
            {
                int dist = Mathf.RoundToInt(_scaler.DistanceTraveled);
                distanceText.text = $"{dist}m";
            }

            // Keep above-crowd counter positioned right above the crowd centroid
            if (_crowd != null && aboveCrowdRoot != null)
            {
                float height = 2.4f + (_crowd.CrowdRadius * 0.25f);
                aboveCrowdRoot.position = _crowd.CentroidPosition + new Vector3(0f, height, 0f);

                if (Camera.main != null)
                {
                    aboveCrowdRoot.rotation = Camera.main.transform.rotation;
                }
            }
        }

        private void OnDestroy()
        {
            if (_crowd != null)
            {
                _crowd.OnCountChanged -= UpdateCountDisplay;
            }
        }
    }
}
