using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace MarbleRace.Runtime.UI
{
    public class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressScale = 0.9f;
        [SerializeField] private float animSpeed = 10f;

        private Vector3 _originalScale;
        private Vector3 _targetScale;
        private Coroutine _animCoroutine;

        private void Awake()
        {
            _originalScale = transform.localScale;
            _targetScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _targetScale = _originalScale * pressScale;
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(AnimateScale());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _targetScale = _originalScale;
            if (_animCoroutine != null) StopCoroutine(_animCoroutine);
            _animCoroutine = StartCoroutine(BounceBack());
        }

        private IEnumerator AnimateScale()
        {
            while (Vector3.Distance(transform.localScale, _targetScale) > 0.01f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, Time.unscaledDeltaTime * animSpeed);
                yield return null;
            }
            transform.localScale = _targetScale;
        }

        private IEnumerator BounceBack()
        {
            // Overshoot slightly for bounce effect
            Vector3 overshoot = _originalScale * 1.05f;
            float elapsed = 0f;

            while (elapsed < 0.1f)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(transform.localScale, overshoot, elapsed / 0.1f);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(transform.localScale, _originalScale, elapsed / 0.1f);
                yield return null;
            }
            transform.localScale = _originalScale;
        }
    }
}
