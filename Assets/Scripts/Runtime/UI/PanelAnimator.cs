using UnityEngine;
using System.Collections;

namespace MarbleRace.Runtime.UI
{
    public class PanelAnimator : MonoBehaviour
    {
        public enum SlideDirection { Left, Right, Up, Down, Scale }

        [SerializeField] private SlideDirection slideIn = SlideDirection.Up;
        [SerializeField] private float animDuration = 0.3f;

        private RectTransform _rect;
        private CanvasGroup _canvasGroup;
        private Vector2 _originalPosition;
        private Coroutine _currentAnim;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _originalPosition = _rect.anchoredPosition;
        }

        private void OnEnable()
        {
            if (_rect == null) return;
            PlayShowAnimation();
        }

        public void PlayShowAnimation()
        {
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _currentAnim = StartCoroutine(AnimateIn());
        }

        public void PlayHideAnimation(System.Action onComplete = null)
        {
            if (_currentAnim != null) StopCoroutine(_currentAnim);
            _currentAnim = StartCoroutine(AnimateOut(onComplete));
        }

        private IEnumerator AnimateIn()
        {
            Vector2 startOffset = GetOffsetForDirection(slideIn);

            float elapsed = 0f;
            _canvasGroup.alpha = 0f;

            if (slideIn == SlideDirection.Scale)
            {
                _rect.localScale = Vector3.one * 0.8f;
                _rect.anchoredPosition = _originalPosition;
            }
            else
            {
                _rect.anchoredPosition = _originalPosition + startOffset;
                _rect.localScale = Vector3.one;
            }

            while (elapsed < animDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutBack(Mathf.Clamp01(elapsed / animDuration));

                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, t * 2f); // Fade in faster

                if (slideIn == SlideDirection.Scale)
                {
                    _rect.localScale = Vector3.Lerp(Vector3.one * 0.8f, Vector3.one, t);
                }
                else
                {
                    _rect.anchoredPosition = Vector2.Lerp(_originalPosition + startOffset, _originalPosition, t);
                }

                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _rect.anchoredPosition = _originalPosition;
            _rect.localScale = Vector3.one;
        }

        private IEnumerator AnimateOut(System.Action onComplete)
        {
            Vector2 endOffset = GetOffsetForDirection(slideIn) * -0.5f;
            Vector3 startScale = _rect.localScale;
            float elapsed = 0f;
            float outDuration = animDuration * 0.6f;

            while (elapsed < outDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / outDuration);

                _canvasGroup.alpha = 1f - t;

                if (slideIn == SlideDirection.Scale)
                {
                    _rect.localScale = Vector3.Lerp(startScale, Vector3.one * 0.9f, t);
                }
                else
                {
                    _rect.anchoredPosition = Vector2.Lerp(_originalPosition, _originalPosition + endOffset, t);
                }

                yield return null;
            }

            _canvasGroup.alpha = 0f;
            onComplete?.Invoke();
        }

        private Vector2 GetOffsetForDirection(SlideDirection dir)
        {
            switch (dir)
            {
                case SlideDirection.Left: return new Vector2(-800f, 0f);
                case SlideDirection.Right: return new Vector2(800f, 0f);
                case SlideDirection.Up: return new Vector2(0f, -600f);
                case SlideDirection.Down: return new Vector2(0f, 600f);
                default: return Vector2.zero;
            }
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
