using UnityEngine;

namespace MarbleRace.Runtime.Marble
{
    public class MarbleParticles : MonoBehaviour
    {
        private ParticleSystem _dustParticles;
        private ParticleSystem _sparkParticles;
        private Rigidbody _rb;
        private bool _isGrounded;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            CreateDustSystem();
            CreateSparkSystem();
        }

        private void CreateDustSystem()
        {
            var dustObj = new GameObject("DustParticles");
            dustObj.transform.SetParent(transform);
            dustObj.transform.localPosition = Vector3.down * 0.4f;

            _dustParticles = dustObj.AddComponent<ParticleSystem>();
            var main = _dustParticles.main;
            main.startLifetime = 0.6f;
            main.startSpeed = 0.5f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.6f, 0.55f, 0.45f, 0.4f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 30;

            var emission = _dustParticles.emission;
            emission.rateOverTime = 0;

            var shape = _dustParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.2f;

            var sizeOverLifetime = _dustParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

            var colorOverLifetime = _dustParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new(Color.white, 0), new(Color.white, 1) },
                new GradientAlphaKey[] { new(0.4f, 0), new(0f, 1) }
            );
            colorOverLifetime.color = grad;

            // Use default particle material
            var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.SetFloat("_Mode", 1); // Additive

            _dustParticles.Stop();
        }

        private void CreateSparkSystem()
        {
            var sparkObj = new GameObject("SparkParticles");
            sparkObj.transform.SetParent(transform);
            sparkObj.transform.localPosition = Vector3.zero;

            _sparkParticles = sparkObj.AddComponent<ParticleSystem>();
            var main = _sparkParticles.main;
            main.startLifetime = 0.3f;
            main.startSpeed = 3f;
            main.startSize = 0.08f;
            main.startColor = new Color(1f, 0.8f, 0.3f, 1f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 20;
            main.gravityModifier = 1f;

            var emission = _sparkParticles.emission;
            emission.rateOverTime = 0;

            var shape = _sparkParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var sizeOverLifetime = _sparkParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

            var renderer = sparkObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.SetFloat("_Mode", 0); // Additive
            renderer.material.color = new Color(1f, 0.8f, 0.3f);

            _sparkParticles.Stop();
        }

        private void Update()
        {
            if (_rb == null) return;

            float speed = _rb.linearVelocity.magnitude;

            // Emit dust when rolling fast on ground
            if (_isGrounded && speed > 2f)
            {
                var emission = _dustParticles.emission;
                emission.rateOverTime = Mathf.Lerp(5, 20, (speed - 2f) / 10f);
                if (!_dustParticles.isPlaying) _dustParticles.Play();
            }
            else
            {
                if (_dustParticles.isPlaying) _dustParticles.Stop();
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            _isGrounded = true;
        }

        private void OnCollisionExit(Collision collision)
        {
            _isGrounded = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            float impact = collision.relativeVelocity.magnitude;
            if (impact > 3f && _sparkParticles != null)
            {
                _sparkParticles.transform.position = collision.contacts[0].point;
                _sparkParticles.Emit(Mathf.RoundToInt(impact * 2));
            }
        }
    }
}
