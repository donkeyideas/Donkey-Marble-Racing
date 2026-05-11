using UnityEngine;
using MarbleRace.Runtime.Marble;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Camera;

namespace MarbleRace.Runtime.Track
{
    [RequireComponent(typeof(Collider))]
    public class FinishLine : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private RaceCamera raceCamera;

        private bool _firstFinishTriggered;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        public void ResetForNewRace()
        {
            _firstFinishTriggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Marble")) return;

            var marble = other.GetComponent<MarbleController>();
            if (marble == null || marble.HasFinished) return;

            marble.MarkFinished();

            // First marble to finish triggers celebration
            if (!_firstFinishTriggered)
            {
                _firstFinishTriggered = true;
                TriggerCelebration(marble);
            }

            raceManager.RegisterFinish(marble);
        }

        private void TriggerCelebration(MarbleController winner)
        {
            // Camera focuses on winner
            if (raceCamera != null)
                raceCamera.FocusOnMarble(winner.transform);

            // Slow-mo for dramatic effect
            Time.timeScale = 0.3f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
    }
}
