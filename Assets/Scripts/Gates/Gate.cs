using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Visuals;

namespace EndlessCrowdRunner.Gates
{
    /// <summary>
    /// An individual gate displaying a math operation (+, -, *, /).
    /// Colors dynamically green (positive) or red (negative).
    /// </summary>
    public class Gate : MonoBehaviour
    {
        [Header("Gate Settings")]
        [SerializeField] private GateType gateType = GateType.Add;
        [SerializeField] private int value = 10;
        [SerializeField] private float gateWidth = 4.0f;

        [Header("Visual Components")]
        [SerializeField] private Renderer frameRenderer;
        [SerializeField] private Renderer panelRenderer;
        [SerializeField] private TextMesh textMesh;

        private GateGroup _parentGroup;
        private bool _isConsumed = false;
        private Vector3 _originalScale = Vector3.one;

        public GateType Type => gateType;
        public int Value => value;
        public float GateWidth => gateWidth;
        public bool IsPositive => gateType == GateType.Add || gateType == GateType.Multiply;
        public bool IsConsumed => _isConsumed;

        private void Awake()
        {
            _originalScale = transform.localScale;
            if (panelRenderer == null) panelRenderer = GetComponentInChildren<Renderer>();
            if (textMesh == null) textMesh = GetComponentInChildren<TextMesh>();
        }

        public void Setup(GateGroup parent, GateType type, int val, float width)
        {
            _parentGroup = parent;
            gateType = type;
            value = Mathf.Max(1, val);
            gateWidth = width;
            _isConsumed = false;
            transform.localScale = _originalScale;

            RefreshVisuals();
        }

        public void RefreshVisuals()
        {
            string label = GetDisplayText();
            if (textMesh != null)
            {
                textMesh.text = label;
                textMesh.color = Color.white;
            }

            Color panelColor = IsPositive ? new Color(0.1f, 0.85f, 0.3f, 0.7f) : new Color(0.95f, 0.2f, 0.2f, 0.7f);
            Color frameColor = IsPositive ? new Color(0.2f, 1.0f, 0.4f, 1.0f) : new Color(1.0f, 0.1f, 0.1f, 1.0f);

            if (panelRenderer != null)
            {
                Material mat = VisualAssetFactory.GetGateMaterial(IsPositive);
                if (mat != null) panelRenderer.sharedMaterial = mat;
            }

            if (frameRenderer != null)
            {
                Material fMat = VisualAssetFactory.GetGateFrameMaterial(IsPositive);
                if (fMat != null) frameRenderer.sharedMaterial = fMat;
            }
        }

        public string GetDisplayText()
        {
            switch (gateType)
            {
                case GateType.Add: return $"+{value}";
                case GateType.Multiply: return $"x{value}";
                case GateType.Subtract: return $"-{value}";
                case GateType.Divide: return $"/{value}";
                default: return $"{value}";
            }
        }

        /// <summary>
        /// Calculates horizontal distance from crowd centroid to this gate center.
        /// </summary>
        public float GetCrowdOverlapScore(Vector3 crowdPos, float crowdRadius)
        {
            float gateCenterX = transform.position.x;
            float dist = Mathf.Abs(crowdPos.x - gateCenterX);
            float maxOverlapDist = (gateWidth * 0.5f) + crowdRadius;

            if (dist > maxOverlapDist) return 0f;
            return 1.0f - (dist / maxOverlapDist);
        }

        /// <summary>
        /// Triggers this gate, applying its math to the player crowd.
        /// </summary>
        public void Trigger(CrowdController crowd)
        {
            if (_isConsumed || crowd == null) return;
            _isConsumed = true;

            // Apply math to crowd
            crowd.ApplyGateOperation(gateType, value, transform.position);

            // Punchy scale animation
            StartCoroutine(AnimateTrigger());
        }

        private System.Collections.IEnumerator AnimateTrigger()
        {
            float elapsed = 0f;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float s = Mathf.PingPong(elapsed * 8f, 0.25f);
                transform.localScale = _originalScale * (1f + s);
                yield return null;
            }

            transform.localScale = _originalScale;

            // Dim gate panel
            if (panelRenderer != null)
            {
                panelRenderer.sharedMaterial = VisualAssetFactory.GetGateConsumedMaterial();
            }
        }

        public void ResetGate()
        {
            _isConsumed = false;
            transform.localScale = _originalScale;
        }
    }
}
