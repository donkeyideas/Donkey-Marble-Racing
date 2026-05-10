using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.Camera
{
    public class RaceCamera : MonoBehaviour
    {
        private Vector3 _offset = new Vector3(0f, 6f, -10f);
        private float _smoothTime = 0.4f;
        private float _rotationSmooth = 3f;

        private List<MarbleController> _marbles;
        private Vector3 _currentVelocity;
        private Vector3 _smoothTarget;
        private bool _active;

        // Shake
        private float _shakeIntensity;
        private float _shakeDuration;
        private float _shakeTimer;

        // Finish focus
        private Transform _finishFocusTarget;
        private float _finishZoomTimer;
        private bool _inFinishMode;

        public void SetMarbles(List<MarbleController> marbles)
        {
            _marbles = marbles;
            _active = true;
            _inFinishMode = false;

            var cam = GetComponent<UnityEngine.Camera>();
            if (cam != null)
                cam.fieldOfView = 60f;

            Vector3 center = GetGroupCenter();
            _smoothTarget = center;
            transform.position = center + _offset;
            transform.LookAt(center);
        }

        private void LateUpdate()
        {
            if (!_active || _marbles == null || _marbles.Count == 0) return;

            if (_inFinishMode && _finishFocusTarget != null)
            {
                UpdateFinishCamera();
                return;
            }

            Vector3 target = GetGroupCenter();
            _smoothTarget = Vector3.SmoothDamp(_smoothTarget, target, ref _currentVelocity, _smoothTime);

            Vector3 desiredPos = _smoothTarget + _offset;
            transform.position = desiredPos;

            Vector3 lookAt = _smoothTarget + Vector3.forward * 6f;
            Quaternion desiredRot = Quaternion.LookRotation(lookAt - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, _rotationSmooth * Time.deltaTime);

            // Apply shake
            if (_shakeTimer > 0)
            {
                _shakeTimer -= Time.deltaTime;
                float intensity = _shakeIntensity * (_shakeTimer / _shakeDuration);
                transform.position += Random.insideUnitSphere * intensity;
            }
        }

        private void UpdateFinishCamera()
        {
            _finishZoomTimer += Time.unscaledDeltaTime;

            Vector3 targetPos = _finishFocusTarget.position;
            float angle = _finishZoomTimer * 30f;
            float dist = Mathf.Lerp(8f, 5f, Mathf.Clamp01(_finishZoomTimer / 3f));
            float height = Mathf.Lerp(5f, 3f, Mathf.Clamp01(_finishZoomTimer / 3f));

            Vector3 orbitPos = targetPos + new Vector3(
                Mathf.Sin(angle * Mathf.Deg2Rad) * dist,
                height,
                Mathf.Cos(angle * Mathf.Deg2Rad) * dist
            );

            transform.position = Vector3.Lerp(transform.position, orbitPos, Time.unscaledDeltaTime * 3f);
            Quaternion lookRot = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.unscaledDeltaTime * 5f);
        }

        public void Shake(float intensity, float duration)
        {
            _shakeIntensity = intensity;
            _shakeDuration = duration;
            _shakeTimer = duration;
        }

        public void FocusOnMarble(Transform marble)
        {
            _finishFocusTarget = marble;
            _inFinishMode = true;
            _finishZoomTimer = 0f;
        }

        public void ResetFinishMode()
        {
            _inFinishMode = false;
            _finishFocusTarget = null;
        }

        private Vector3 GetGroupCenter()
        {
            Vector3 sum = Vector3.zero;
            float maxZ = float.MinValue;
            int count = 0;

            foreach (var marble in _marbles)
            {
                if (marble == null) continue;
                // Ignore marbles that fell off the track (way below expected Y)
                if (marble.transform.position.y < -15f) continue;
                sum += marble.transform.position;
                if (marble.transform.position.z > maxZ)
                    maxZ = marble.transform.position.z;
                count++;
            }

            if (count == 0) return _smoothTarget;

            Vector3 center = sum / count;
            center.z = Mathf.Lerp(center.z, maxZ, 0.6f);
            return center;
        }
    }
}
