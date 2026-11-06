using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Pooling;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// Mobile diagnostic overlay rendering real-time FPS, unit count, and memory metrics.
    /// Uses IMGUI for zero Canvas overhead or can render directly.
    /// </summary>
    public class DebugFPSOverlay : MonoBehaviour
    {
        [SerializeField] private bool showOverlay = true;

        private float _deltaTime = 0f;
        private float _fps = 0f;
        private float _minFps = 999f;
        private float _maxFps = 0f;
        private float _fpsSampleTimer = 0f;
        private GUIStyle _style;

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            _deltaTime += (dt - _deltaTime) * 0.1f;
            _fps = 1.0f / _deltaTime;

            _fpsSampleTimer += dt;
            if (_fpsSampleTimer > 1.0f)
            {
                _fpsSampleTimer = 0f;
                if (_fps < _minFps && _fps > 5f) _minFps = _fps;
                if (_fps > _maxFps) _maxFps = _fps;
            }

            // Triple tap top-left corner on mobile to toggle debug overlay
            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Vector2 pos = Input.GetTouch(0).position;
                if (pos.x < 150f && pos.y > Screen.height - 150f)
                {
                    showOverlay = !showOverlay;
                }
            }
        }

        private void OnGUI()
        {
            if (!showOverlay) return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold
                };
            }

            Color fpsColor = (_fps >= 55f) ? Color.green : ((_fps >= 30f) ? Color.yellow : Color.red);
            _style.normal.textColor = fpsColor;

            int playerCount = 0;
            var crowd = FindObjectOfType<CrowdController>();
            if (crowd != null) playerCount = crowd.CurrentCount;

            int enemyCount = 0;
            if (PoolManager.Instance != null && PoolManager.Instance.EnemyUnits != null)
            {
                enemyCount = PoolManager.Instance.EnemyUnits.ActiveCount;
            }

            int bullets = 0;
            if (PoolManager.Instance != null && PoolManager.Instance.Bullets != null)
            {
                bullets = PoolManager.Instance.Bullets.ActiveCount;
            }

            float dist = crowd != null ? crowd.CentroidPosition.z : 0f;

            string info = $"FPS: {_fps:0.0} (Min: {_minFps:0.0} / Max: {_maxFps:0.0})\n" +
                          $"Stickmen: Player={playerCount} | Enemy={enemyCount} | Total={playerCount + enemyCount}\n" +
                          $"Active Bullets: {bullets}\n" +
                          $"Distance: {dist:0}m";

            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.Box(new Rect(15, 15, 380, 115), GUIContent.none);
            GUI.color = Color.white;

            GUI.Label(new Rect(25, 20, 360, 100), info, _style);
        }
    }
}
