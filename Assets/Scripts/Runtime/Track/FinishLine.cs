using UnityEngine;
using MarbleRace.Runtime.Marble;
using MarbleRace.Runtime.Managers;

namespace MarbleRace.Runtime.Track
{
    [RequireComponent(typeof(Collider))]
    public class FinishLine : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;

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

            marble.MarkFinished();
            raceManager.RegisterFinish(marble);
        }
    }
}
