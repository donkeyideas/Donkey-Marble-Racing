using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Data;

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

            int spawnCount = Mathf.Min(count, marbleConfigs.Length, spawnPoints.Length);

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 position = spawnPoints[i].position;
                Quaternion rotation = spawnPoints[i].rotation;

                GameObject marbleObj = Instantiate(marblePrefab, position, rotation);
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
