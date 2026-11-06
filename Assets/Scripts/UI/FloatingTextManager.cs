using UnityEngine;
using EndlessCrowdRunner.Pooling;

namespace EndlessCrowdRunner.UI
{
    /// <summary>
    /// Singleton coordinator for spawning pooled floating combat and gate texts.
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnFloatingText(string text, Vector3 worldPos, Color color)
        {
            if (PoolManager.Instance == null || PoolManager.Instance.FloatingTexts == null) return;

            FloatingText ft = PoolManager.Instance.FloatingTexts.Get(worldPos, Quaternion.identity);
            ft.Setup(text, color);
        }
    }
}
