using UnityEngine;
using MarbleRace.Data;

namespace MarbleRace.Runtime.Marble
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class MarbleController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private RaceSettings raceSettings;

        private Rigidbody _rb;
        private MarbleIdentity _identity;
        private float _nextNudgeTime;
        private bool _isFrozen = true;
        private bool _hasFinished;

        // Personality modifiers from MarbleData
        private float _massMultiplier = 1f;
        private float _dragMultiplier = 0.1f;
        private float _bounciness = 0.6f;
        private float _nudgeStrength = 1f;

        public string MarbleId => _identity != null ? _identity.MarbleId : "";
        public bool HasFinished => _hasFinished;
        public float Speed => _rb != null ? _rb.linearVelocity.magnitude : 0f;
        public Rigidbody Rb => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _identity = GetComponent<MarbleIdentity>();
        }

        public void Initialize(RaceSettings settings)
        {
            raceSettings = settings;

            // Apply personality from MarbleData
            if (_identity != null && _identity.Data != null)
            {
                var data = _identity.Data;
                _massMultiplier = data.massMultiplier;
                _dragMultiplier = data.dragMultiplier;
                _bounciness = data.bounciness;
                _nudgeStrength = data.nudgeStrength;
            }

            ConfigureRigidbody();
            ScheduleNextNudge();
        }

        public void Freeze()
        {
            _isFrozen = true;
            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.isKinematic = true;
            }
        }

        public void Release()
        {
            _isFrozen = false;
            _hasFinished = false;
            if (_rb != null)
            {
                _rb.isKinematic = false;
                // Initial launch boost for exciting start
                float boost = Random.Range(1.5f, 2.5f) * _nudgeStrength;
                _rb.AddForce(Vector3.forward * boost, ForceMode.Impulse);
            }
            ScheduleNextNudge();
        }

        public void MarkFinished()
        {
            _hasFinished = true;
        }

        private void FixedUpdate()
        {
            if (_isFrozen || _hasFinished) return;

            if (Time.time >= _nextNudgeTime)
            {
                ApplyRandomNudge();
                ScheduleNextNudge();
            }

            // Clamp max speed to prevent marbles from flying off
            if (_rb.linearVelocity.magnitude > 18f)
            {
                _rb.linearVelocity = _rb.linearVelocity.normalized * 18f;
            }
        }

        private void ApplyRandomNudge()
        {
            if (raceSettings == null || _rb == null) return;

            // Forward force along world Z (the track direction)
            float forwardForce = Random.Range(raceSettings.minNudgeForce, raceSettings.maxNudgeForce);
            forwardForce *= _nudgeStrength;
            Vector3 force = Vector3.forward * forwardForce;

            // Small lateral randomness for variety (world X)
            float lateral = Random.Range(-raceSettings.lateralNudgeStrength, raceSettings.lateralNudgeStrength) * 0.5f;
            lateral *= _nudgeStrength;
            force += Vector3.right * lateral;

            // No upward force — gravity and track slope are enough
            _rb.AddForce(force, ForceMode.Impulse);

            // Gentle torque for natural rolling
            Vector3 torque = new Vector3(
                Random.Range(-0.5f, 0.5f),
                0f,
                Random.Range(-0.5f, 0.5f)
            ) * forwardForce * 0.3f;
            _rb.AddTorque(torque, ForceMode.Impulse);
        }

        private void ScheduleNextNudge()
        {
            if (raceSettings == null) return;
            _nextNudgeTime = Time.time + Random.Range(raceSettings.minNudgeInterval, raceSettings.maxNudgeInterval);
        }

        private void ConfigureRigidbody()
        {
            if (_rb == null || raceSettings == null) return;

            _rb.mass = raceSettings.marbleMass * _massMultiplier;
            _rb.linearDamping = _dragMultiplier;
            _rb.angularDamping = raceSettings.marbleAngularDrag;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // Apply bounciness to the collider's physics material
            var col = GetComponent<SphereCollider>();
            if (col != null && col.material != null)
            {
                col.material.bounciness = _bounciness;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Marble"))
            {
                float impactForce = collision.relativeVelocity.magnitude;
                if (impactForce > 1f)
                {
                    Managers.AudioManager.Instance?.PlayMarbleCollision();
                }
                // Camera shake on big impacts
                if (impactForce > 3f)
                {
                    var cam = FindAnyObjectByType<Camera.RaceCamera>();
                    if (cam != null)
                        cam.Shake(impactForce * 0.02f, 0.15f);
                }
            }
        }
    }
}
