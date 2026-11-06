using System.Collections.Generic;
using UnityEngine;
using EndlessCrowdRunner.Crowd;
using EndlessCrowdRunner.Difficulty;

namespace EndlessCrowdRunner.Gates
{
    /// <summary>
    /// Holds a row of 2 or 3 gates side-by-side. Coordinates single-choice triggering:
    /// when the crowd passes, the gate with the greatest overlap is chosen.
    /// </summary>
    public class GateGroup : MonoBehaviour
    {
        [Header("Gates")]
        [SerializeField] private List<Gate> gates = new List<Gate>();
        [SerializeField] private float triggerZThreshold = 1.0f;

        private bool _isTriggered = false;

        public IReadOnlyList<Gate> Gates => gates;
        public bool IsTriggered => _isTriggered;

        public void Initialize(List<Gate> gateList)
        {
            gates = gateList;
            _isTriggered = false;
        }

        public void ResetGroup()
        {
            _isTriggered = false;
            for (int i = 0; i < gates.Count; i++)
            {
                if (gates[i] != null)
                {
                    gates[i].ResetGate();
                }
            }
        }

        private void Update()
        {
            if (_isTriggered) return;

            // Find player crowd
            CrowdController crowd = FindObjectOfType<CrowdController>();
            if (crowd == null) return;

            Vector3 crowdPos = crowd.CentroidPosition;
            float gateZ = transform.position.z;

            // Check if crowd has reached the gate line along Z
            if (crowdPos.z >= gateZ - triggerZThreshold && crowdPos.z <= gateZ + triggerZThreshold + 1.5f)
            {
                ResolveGateSelection(crowd);
            }
        }

        /// <summary>
        /// Selects the gate overlapping the majority of the player crowd.
        /// </summary>
        private void ResolveGateSelection(CrowdController crowd)
        {
            if (_isTriggered || gates.Count == 0) return;

            Gate bestGate = null;
            float maxScore = -1f;

            for (int i = 0; i < gates.Count; i++)
            {
                Gate g = gates[i];
                if (g == null) continue;

                float score = g.GetCrowdOverlapScore(crowd.CentroidPosition, crowd.CrowdRadius);
                if (score > maxScore)
                {
                    maxScore = score;
                    bestGate = g;
                }
            }

            // Fallback to nearest gate center if overlap score is very close
            if (bestGate == null)
            {
                float minDist = float.MaxValue;
                for (int i = 0; i < gates.Count; i++)
                {
                    float d = Mathf.Abs(crowd.CentroidPosition.x - gates[i].transform.position.x);
                    if (d < minDist)
                    {
                        minDist = d;
                        bestGate = gates[i];
                    }
                }
            }

            if (bestGate != null)
            {
                _isTriggered = true;
                bestGate.Trigger(crowd);
            }
        }

        /// <summary>
        /// Applies fairness rules: ensures at least one option is positive / survivable.
        /// </summary>
        public void EnforceFairness(int currentEstimatedPlayerCount)
        {
            bool hasSurvivable = false;
            for (int i = 0; i < gates.Count; i++)
            {
                Gate g = gates[i];
                if (g.IsPositive)
                {
                    hasSurvivable = true;
                    break;
                }
                // Even negative is survivable if it doesn't wipe out the whole crowd
                if (g.Type == GateType.Subtract && g.Value < currentEstimatedPlayerCount)
                {
                    hasSurvivable = true;
                    break;
                }
                if (g.Type == GateType.Divide && (currentEstimatedPlayerCount / g.Value) > 0)
                {
                    hasSurvivable = true;
                    break;
                }
            }

            // If completely fatal, turn the first gate into a guaranteed positive option
            if (!hasSurvivable && gates.Count > 0)
            {
                gates[0].Setup(this, GateType.Add, 10, gates[0].GateWidth);
            }
        }
    }
}
