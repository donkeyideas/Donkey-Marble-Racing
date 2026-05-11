using UnityEngine;
using MarbleRace.Runtime.Marble;
using MarbleRace.Runtime.Managers;
using MarbleRace.Runtime.Camera;

namespace MarbleRace.Runtime.Track
{
    /// <summary>
    /// Kept for backwards compatibility. Finish detection is now handled
    /// by RaceManager using position-based checks in Update().
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FinishLine : MonoBehaviour
    {
        [SerializeField] private RaceManager raceManager;
        [SerializeField] private RaceCamera raceCamera;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        public void ResetForNewRace() { }
    }
}
