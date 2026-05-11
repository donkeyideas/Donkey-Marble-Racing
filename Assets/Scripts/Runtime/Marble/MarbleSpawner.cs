using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Data;
using MarbleRace.Runtime.Track;

namespace MarbleRace.Runtime.Marble
{
    public class MarbleSpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RaceSettings raceSettings;
        [SerializeField] private GameObject marblePrefab;
        [SerializeField] private MarbleData[] marbleConfigs;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] spawnPoints;

        private List<MarbleController> _spawnedMarbles = new List<MarbleController>();

        public List<MarbleController> SpawnMarbles(int count, TrackData track)
        {
            DespawnAll();

            int spawnCount = Mathf.Min(count, marbleConfigs.Length);

            // Generate spawn positions on the track start area
            var positions = GenerateSpawnPositions(spawnCount);

            // Shuffle so marbles start in random lane positions each race
            for (int i = positions.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                var temp = positions[i];
                positions[i] = positions[j];
                positions[j] = temp;
            }

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject marbleObj = Instantiate(marblePrefab, positions[i], Quaternion.identity);
                marbleObj.name = $"Marble_{marbleConfigs[i].marbleName}";
                marbleObj.tag = "Marble";

                // Setup identity
                var identity = marbleObj.GetComponent<MarbleIdentity>();
                if (identity != null)
                    identity.Setup(marbleConfigs[i]);

                // Setup controller
                var controller = marbleObj.GetComponent<MarbleController>();
                if (controller != null)
                    controller.Initialize(raceSettings);

                _spawnedMarbles.Add(controller);
            }

            return _spawnedMarbles;
        }

        private Vector3[] GenerateSpawnPositions(int count)
        {
            // Place marbles on the track surface near the start
            // Use two points along the track to determine direction
            Vector3 trackStart = RuntimeTrackBuilder.CurvePoint(0f);
            Vector3 trackAhead = RuntimeTrackBuilder.CurvePoint(0.02f);
            Vector3 forward = (trackAhead - trackStart).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // If right is zero (track goes straight up), fallback
            if (right.sqrMagnitude < 0.01f)
                right = Vector3.right;

            float surfaceY = trackStart.y + 0.75f; // floor top + marble radius

            float trackWidth = 5f;
            float usableWidth = trackWidth - 1.5f; // leave margin from walls

            var positions = new Vector3[count];
            int cols = 4; // 4 marbles per row
            for (int i = 0; i < count; i++)
            {
                int row = i / cols;
                int col = i % cols;

                // Spread across track width using the local right direction
                float lateralOffset = Mathf.Lerp(-usableWidth / 2f, usableWidth / 2f, (col + 0.5f) / cols);
                // Stagger rows along the track forward direction
                float forwardOffset = 1f + row * 1.2f;

                Vector3 pos = trackStart + right * lateralOffset + forward * forwardOffset;
                pos.y = surfaceY; // Keep consistent height on track surface
                positions[i] = pos;
            }

            return positions;
        }

        public void DespawnAll()
        {
            foreach (var marble in _spawnedMarbles)
            {
                if (marble != null)
                    Destroy(marble.gameObject);
            }
            _spawnedMarbles.Clear();
        }

        public List<MarbleController> GetSpawnedMarbles()
        {
            return _spawnedMarbles;
        }
    }
}
