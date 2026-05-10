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
        private ParticleSystem _confetti;

        private void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            CreateConfetti();
        }

        public void ResetForNewRace()
        {
            _firstFinishTriggered = false;
        }

        private void CreateConfetti()
        {
            var confettiObj = new GameObject("Confetti");
            confettiObj.transform.SetParent(transform);
            confettiObj.transform.localPosition = Vector3.up * 3f;

            _confetti = confettiObj.AddComponent<ParticleSystem>();
            var main = _confetti.main;
            main.startLifetime = 2.5f;
            main.startSpeed = 5f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.25f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 200;
            main.gravityModifier = 0.8f;

            // Multi-color confetti
            var colorOverLifetime = _confetti.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new(Color.red, 0f),
                    new(Color.yellow, 0.25f),
                    new(Color.green, 0.5f),
                    new(Color.blue, 0.75f),
                    new(Color.magenta, 1f)
                },
                new GradientAlphaKey[] { new(1f, 0f), new(1f, 0.8f), new(0f, 1f) }
            );
            colorOverLifetime.color = grad;

            // Random start color
            main.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.cyan);

            var emission = _confetti.emission;
            emission.rateOverTime = 0;

            var shape = _confetti.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 2f;

            var renderer = confettiObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            _confetti.Stop();
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
            // Confetti burst
            if (_confetti != null)
                _confetti.Emit(100);

            // Camera focuses on winner with orbit
            if (raceCamera != null)
                raceCamera.FocusOnMarble(winner.transform);

            // Slow-mo for dramatic effect
            Time.timeScale = 0.3f;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            // Camera shake
            if (raceCamera != null)
                raceCamera.Shake(0.2f, 0.5f);
        }
    }
}
