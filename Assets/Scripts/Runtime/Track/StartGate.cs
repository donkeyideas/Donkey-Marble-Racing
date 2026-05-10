using UnityEngine;

namespace MarbleRace.Runtime.Track
{
    public class StartGate : MonoBehaviour
    {
        [SerializeField] private GameObject gateVisual;
        [SerializeField] private Collider gateCollider;
        [SerializeField] private float openAnimationDuration = 0.5f;

        private bool _isOpen;

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;

            // Disable the collider so marbles can pass
            if (gateCollider != null)
                gateCollider.enabled = false;

            // Animate gate opening (simple scale-down)
            if (gateVisual != null)
                StartCoroutine(AnimateOpen());
        }

        public void Close()
        {
            _isOpen = false;

            if (gateCollider != null)
                gateCollider.enabled = true;

            if (gateVisual != null)
                gateVisual.transform.localScale = Vector3.one;
        }

        private System.Collections.IEnumerator AnimateOpen()
        {
            float elapsed = 0f;
            Vector3 startScale = gateVisual.transform.localScale;
            Vector3 endScale = new Vector3(startScale.x, 0f, startScale.z);

            while (elapsed < openAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / openAnimationDuration;
                gateVisual.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                yield return null;
            }

            gateVisual.SetActive(false);
        }
    }
}
