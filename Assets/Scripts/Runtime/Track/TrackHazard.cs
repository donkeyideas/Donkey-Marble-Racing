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
                    break;

                case HazardType.Bumper:
                    Vector3 awayDir = (other.transform.position - transform.position).normalized;
                    rb.AddForce(awayDir * forceStrength, ForceMode.Impulse);
                    break;

                case HazardType.Spinner:
                    Vector3 hitDir = (other.transform.position - transform.position).normalized;
                    rb.AddForce(hitDir * forceStrength, ForceMode.Impulse);
                    break;

                case HazardType.JumpRamp:
                    rb.AddForce(Vector3.up * forceStrength + transform.forward * (forceStrength * 0.5f), ForceMode.Impulse);
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
            }
        }
    }
}
