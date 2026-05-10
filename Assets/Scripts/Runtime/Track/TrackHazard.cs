using UnityEngine;

namespace MarbleRace.Runtime.Track
{
    public enum HazardType
    {
        BoostPad,
        Bumper,
        Spinner,
        Hammer,
        FallingPlatform,
        JumpRamp
    }

    public class TrackHazard : MonoBehaviour
    {
        [SerializeField] private HazardType hazardType;
        [SerializeField] private float forceStrength = 5f;
        [SerializeField] private Vector3 forceDirection = Vector3.forward;
        [SerializeField] private float spinSpeed = 90f;

        private ParticleSystem _hitParticles;

        private void Awake()
        {
            CreateHitParticles();
        }

        private void CreateHitParticles()
        {
            var particleObj = new GameObject("HitEffect");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;

            _hitParticles = particleObj.AddComponent<ParticleSystem>();
            var main = _hitParticles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 4f;
            main.startSize = 0.12f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 30;
            main.gravityModifier = 0.5f;

            // Color based on hazard type
            switch (hazardType)
            {
                case HazardType.BoostPad:
                    main.startColor = new Color(0.2f, 1f, 0.4f, 1f);
                    break;
                case HazardType.Bumper:
                    main.startColor = new Color(1f, 0.3f, 0.1f, 1f);
                    break;
                case HazardType.Spinner:
                    main.startColor = new Color(1f, 0.7f, 0f, 1f);
                    break;
                default:
                    main.startColor = new Color(1f, 1f, 1f, 0.8f);
                    break;
            }

            var emission = _hitParticles.emission;
            emission.rateOverTime = 0;

            var shape = _hitParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var sizeOverLifetime = _hitParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));

            var renderer = particleObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

            _hitParticles.Stop();
        }

        private void EmitHitEffect(Vector3 position, int count = 12)
        {
            if (_hitParticles == null) return;
            _hitParticles.transform.position = position;
            _hitParticles.Emit(count);
        }

        private void Update()
        {
            // Animate spinning hazards
            if (hazardType == HazardType.Spinner || hazardType == HazardType.Hammer)
            {
                transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Marble")) return;

            var rb = other.GetComponent<Rigidbody>();
            if (rb == null) return;

            switch (hazardType)
            {
                case HazardType.BoostPad:
                    rb.AddForce(transform.TransformDirection(forceDirection) * forceStrength, ForceMode.Impulse);
                    EmitHitEffect(other.transform.position, 8);
                    break;

                case HazardType.Bumper:
                    Vector3 awayDir = (other.transform.position - transform.position).normalized;
                    rb.AddForce(awayDir * forceStrength, ForceMode.Impulse);
                    EmitHitEffect(other.transform.position, 15);
                    break;

                case HazardType.Spinner:
                    Vector3 hitDir = (other.transform.position - transform.position).normalized;
                    rb.AddForce(hitDir * forceStrength, ForceMode.Impulse);
                    EmitHitEffect(other.transform.position, 12);
                    break;

                case HazardType.JumpRamp:
                    rb.AddForce(Vector3.up * forceStrength + transform.forward * (forceStrength * 0.5f), ForceMode.Impulse);
                    EmitHitEffect(other.transform.position, 10);
                    break;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Marble")) return;

            var rb = collision.rigidbody;
            if (rb == null) return;

            // Hammers and spinners apply force on collision
            if (hazardType == HazardType.Hammer || hazardType == HazardType.Spinner)
            {
                Vector3 hitDir = (collision.transform.position - transform.position).normalized;
                rb.AddForce(hitDir * forceStrength, ForceMode.Impulse);
                EmitHitEffect(collision.contacts[0].point, 12);
            }
            else if (hazardType == HazardType.Bumper)
            {
                Vector3 awayDir = (collision.transform.position - transform.position).normalized;
                rb.AddForce(awayDir * forceStrength, ForceMode.Impulse);
                EmitHitEffect(collision.contacts[0].point, 15);
            }
        }
    }
}
