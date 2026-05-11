using UnityEngine;

namespace MarbleRace.Runtime.UI
{
    public class WinCelebration : MonoBehaviour
    {
        private ParticleSystem _confetti;

        private void Awake()
        {
            _confetti = CreateConfettiSystem();
            _confetti.transform.SetParent(transform);
            _confetti.transform.localPosition = Vector3.zero;
        }

        public void Play(Vector3 worldPosition)
        {
            transform.position = worldPosition + Vector3.up * 2f;
            _confetti.Play();
        }

        public void Stop()
        {
            if (_confetti != null)
                _confetti.Stop();
        }

        private ParticleSystem CreateConfettiSystem()
        {
            var go = new GameObject("Confetti");
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 2f;
            main.startLifetime = 2.5f;
            main.startSpeed = 8f;
            main.startSize = 0.15f;
            main.maxParticles = 200;
            main.loop = false;
            main.gravityModifier = 1.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.8f, 0f), // gold
                new Color(0f, 1f, 0.5f)  // green
            );

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 100) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;
            shape.radius = 0.5f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = Color.white;

            ps.Stop();
            return ps;
        }
    }
}
