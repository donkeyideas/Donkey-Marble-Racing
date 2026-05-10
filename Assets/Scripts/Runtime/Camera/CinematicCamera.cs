using System.Collections;
using UnityEngine;

namespace MarbleRace.Runtime.Camera
{
    public class CinematicCamera : MonoBehaviour
    {
        [Header("Intro Flyover")]
        [SerializeField] private Transform[] flyoverWaypoints;
        [SerializeField] private float flyoverSpeed = 3f;
        [SerializeField] private float flyoverDuration = 4f;

        [Header("Finish")]
        [SerializeField] private float finishZoomDistance = 3f;
        [SerializeField] private float finishOrbitSpeed = 30f;

        [Header("References")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        private bool _isAnimating;

        public bool IsAnimating => _isAnimating;

        public void PlayIntroFlyover(System.Action onComplete = null)
        {
            if (flyoverWaypoints == null || flyoverWaypoints.Length < 2)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(IntroFlyoverRoutine(onComplete));
        }

        public void PlayFinishCelebration(Transform winner)
        {
            if (winner == null) return;
            StartCoroutine(FinishCelebrationRoutine(winner));
        }

        private IEnumerator IntroFlyoverRoutine(System.Action onComplete)
        {
            _isAnimating = true;
            float elapsed = 0f;

            while (elapsed < flyoverDuration && flyoverWaypoints.Length >= 2)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flyoverDuration;

                // Smooth progression through waypoints
                int segmentCount = flyoverWaypoints.Length - 1;
                float scaledT = t * segmentCount;
                int segment = Mathf.Min((int)scaledT, segmentCount - 1);
                float segmentT = scaledT - segment;

                Vector3 pos = Vector3.Lerp(
                    flyoverWaypoints[segment].position,
                    flyoverWaypoints[segment + 1].position,
                    segmentT
                );

                Quaternion rot = Quaternion.Slerp(
                    flyoverWaypoints[segment].rotation,
                    flyoverWaypoints[segment + 1].rotation,
                    segmentT
                );

                transform.position = pos;
                transform.rotation = rot;

                yield return null;
            }

            _isAnimating = false;
            onComplete?.Invoke();
        }

        private IEnumerator FinishCelebrationRoutine(Transform winner)
        {
            _isAnimating = true;
            float angle = 0f;
            float duration = 3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                angle += finishOrbitSpeed * Time.unscaledDeltaTime;

                Vector3 orbitPos = winner.position + Quaternion.Euler(20f, angle, 0f) * (Vector3.back * finishZoomDistance);
                transform.position = orbitPos;
                transform.LookAt(winner.position);

                yield return null;
            }

            _isAnimating = false;
        }
    }
}
