using UnityEngine;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.Track
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private int checkpointIndex;

        public int Index => checkpointIndex;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Marble")) return;

            var marble = other.GetComponent<MarbleController>();
            if (marble == null || marble.HasFinished) return;

            // Track progress (can be used for position tracking)
            Debug.Log($"{marble.MarbleId} passed checkpoint {checkpointIndex}");
        }
    }
}
