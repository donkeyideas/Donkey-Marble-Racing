using System.Collections.Generic;
using UnityEngine;
using MarbleRace.Data;
using MarbleRace.Runtime.Marble;

namespace MarbleRace.Runtime.Camera
{
    public class RaceCamera : MonoBehaviour
    {
        private Vector3 _offset = new Vector3(0f, 6f, -10f);
        private Vector3 _defaultOffset = new Vector3(0f, 6f, -10f);
        private Vector3 _serpentineOffset = new Vector3(0f, 18f, -16f); // Higher for wide S-curves
        private float _smoothTime = 0.4f;
        private float _rotationSmooth = 3f;

        private List<MarbleController> _marbles;
        private Vector3 _currentVelocity;
        private Vector3 _smoothTarget;
        private bool _active;
        private bool _isStaticCamera; // For racetrack: fixed overhead view

        // Shake
        private float _shakeIntensity;
        private float _shakeDuration;
        private float _shakeTimer;

        // Finish focus
        private Transform _finishFocusTarget;
        private float _finishZoomTimer;
        private bool _inFinishMode;

        /// <summary>
        /// Call before SetMarbles to configure camera for the current track type.
        /// </summary>
        public void SetTrackType(MarbleRace.Data.TrackType trackType)
        {
            var cam = GetComponent<UnityEngine.Camera>();
            _isStaticCamera = false;
            switch (trackType)
            {
                case MarbleRace.Data.TrackType.Serpentine:
                    _offset = _serpentineOffset;
                    if (cam != null) cam.fieldOfView = 75f;
                    break;
                case MarbleRace.Data.TrackType.Racetrack:
                    // Fixed bird's-eye above oval center (0, y, 40)
                    _isStaticCamera = true;
                    if (cam != null) cam.fieldOfView = 70f;
                    break;
                default:
                    _offset = _defaultOffset;
                    if (cam != null) cam.fieldOfView = 60f;
                    break;
            }
        }

        public void SetMarbles(List<MarbleController> marbles)
        {
            _marbles = marbles;
            _active = true;
            _inFinishMode = false;

            if (_isStaticCamera)
            {
                // Fixed position above oval center looking straight down
                transform.position = new Vector3(0f, 60f, 40f);
                transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                Vector3 center = GetGroupCenter();
                _smoothTarget = center;
                transform.position = center + _offset;
                transform.LookAt(center);
            }
        }

        private void LateUpdate()
        {
            if (!_active || _marbles == null || _marbles.Count == 0) return;

            if (_inFinishMode && _finishFocusTarget != null)
            {
                UpdateFinishCamera();
                return;
            }

            // Static camera: stays fixed overhead (racetrack)
            if (_isStaticCamera) return;

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
            // Position camera high above and past the finish, looking back at incoming marbles
            Vector3 targetPos = _finishFocusTarget.position;
            Vector3 desiredPos = targetPos + new Vector3(0f, 10f, 12f);
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * 3f);

            // Look back toward the track (negative z) to see remaining marbles coming
            Vector3 lookTarget = targetPos + new Vector3(0f, 0f, -20f);
            Quaternion desiredRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, Time.deltaTime * 4f);
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
                // Ignore marbles that fell off the track — bucket floor is ~y=-7
                if (marble.transform.position.y < -9f) continue;
                // Ignore marbles that flew way off to the sides
                if (Mathf.Abs(marble.transform.position.x) > 15f) continue;
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
