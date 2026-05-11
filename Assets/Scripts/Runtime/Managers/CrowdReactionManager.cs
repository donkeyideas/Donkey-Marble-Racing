using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.Managers
{
    /// <summary>
    /// Monitors race state and triggers crowd audio reactions on lead changes,
    /// close packs, and near-finish excitement.
    /// </summary>
    public class CrowdReactionManager : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;

        private string _lastLeaderId;
        private int _leadChangeCount;
        private float _lastReactionTime;
        private float _reactionCooldown = 2f;

        private void Update()
        {
            if (raceManager == null || !raceManager.IsRacing) return;

            var marbles = raceManager.ActiveMarbles;
            if (marbles == null || marbles.Count == 0) return;

            // Find current leader
            MarbleController leader = null;
            float maxZ = float.MinValue;
            foreach (var marble in marbles)
            {
                if (marble == null || marble.HasFinished) continue;
                if (marble.transform.position.z > maxZ)
                {
                    maxZ = marble.transform.position.z;
                    leader = marble;
                }
            }

            if (leader == null) return;

            // Ramp crowd intensity based on race progress
            float progress = Mathf.Clamp01(maxZ / 78f);
            AudioManager.Instance?.SetCrowdIntensity(progress);

            // Detect lead change
            string currentLeaderId = leader.MarbleId;
            if (!string.IsNullOrEmpty(_lastLeaderId) && currentLeaderId != _lastLeaderId)
            {
                _leadChangeCount++;
                if (Time.time - _lastReactionTime > _reactionCooldown)
                {
                    AudioManager.Instance?.PlayCrowdGasp();
                    HapticManager.LightTap();
                    _lastReactionTime = Time.time;
                }
            }
            _lastLeaderId = currentLeaderId;

            // Close pack detection — gasp when top 3 are within 1m of each other
            if (Time.time - _lastReactionTime > _reactionCooldown && progress > 0.4f)
            {
                var sorted = new List<MarbleController>(marbles);
                sorted.RemoveAll(m => m == null || m.HasFinished);
                sorted.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));

                if (sorted.Count >= 3)
                {
                    float gap = sorted[0].transform.position.z - sorted[2].transform.position.z;
                    if (gap < 1.5f)
                    {
                        AudioManager.Instance?.PlayCrowdGasp();
                        _lastReactionTime = Time.time;
                    }
                }
            }
        }

        public void ResetForNewRace()
        {
            _lastLeaderId = null;
            _leadChangeCount = 0;
            _lastReactionTime = 0f;
        }
    }
}
