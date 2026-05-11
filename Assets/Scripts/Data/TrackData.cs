using UnityEngine;

namespace MarbleRace.Data
{
    public enum TrackType
    {
        Downhill,
        Zigzag,
        Funnel,
        MultiPath,
        Spiral,
        Serpentine,
        Racetrack
    }

    [CreateAssetMenu(menuName = "MarbleRace/Data/Track Data")]
    public class TrackData : ScriptableObject
    {
        [Header("Track Info")]
        public string trackId;
        public string trackName;
        public TrackType trackType = TrackType.Downhill;

        [Header("Settings")]
        public int checkpointCount = 5;
        public float expectedRaceTime = 30f;

        [Header("Spawn")]
        public Vector3[] startPositions = new Vector3[8];
        public Vector3 startDirection = Vector3.forward;

        [Header("Visual")]
        public Color accentColor = Color.cyan;
        public Color trackColor = new Color(0.2f, 0.2f, 0.3f);
    }
}
