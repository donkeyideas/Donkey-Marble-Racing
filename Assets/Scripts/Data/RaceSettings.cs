using UnityEngine;

namespace MarbleRace.Data
{
    [CreateAssetMenu(menuName = "MarbleRace/Data/Race Settings")]
    public class RaceSettings : ScriptableObject
    {
        [Header("Race Timing")]
        public float countdownDuration = 3f;
        public float bettingDuration = 15f;
        public float raceTimeout = 60f;
        public float photoFinishSlowMo = 0.3f;

        [Header("Marble Physics")]
        public int marbleCount = 8;
        public float marbleMass = 1f;
        public float marbleDrag = 0.1f;
        public float marbleAngularDrag = 0.5f;
        public float marbleBounce = 0.6f;

        [Header("Random Nudge Forces")]
        public float minNudgeInterval = 0.8f;
        public float maxNudgeInterval = 2.0f;
        public float minNudgeForce = 0.1f;
        public float maxNudgeForce = 0.5f;
        public float lateralNudgeStrength = 0.15f;

        [Header("Camera")]
        public float cameraFollowSpeed = 5f;
        public float cameraLookAhead = 2f;
    }
}
