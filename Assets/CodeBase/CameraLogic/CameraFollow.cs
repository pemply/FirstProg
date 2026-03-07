using UnityEngine;

namespace CodeBase.CameraLogic
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private float _rotationAngleX = 40f;
        [SerializeField] private float _distanceOffset = 15f;
        [SerializeField] private float _offsetY = 0.6f;

        private Transform _following;

        private float _shakeTime;
        private float _shakeDuration;
        private float _shakeAmplitude;

        private void LateUpdate()
        {
            if (_following == null)
                return;

            if (CombatJuice.GamePause.IsHardPaused)
                CancelShake();

            Quaternion rotation = Quaternion.Euler(_rotationAngleX, 0f, 0f);

            Vector3 position =
                rotation * new Vector3(0f, 0f, -_distanceOffset) +
                FollowingPointPosition();

            position += GetShakeOffset();

            transform.SetPositionAndRotation(position, rotation);
        }

        public void Follow(GameObject followingObject)
        {
            _following = followingObject.transform;
        }

        public void Shake(float amplitude, float duration)
        {
            if (CombatJuice.GamePause.IsHardPaused)
                return;

            _shakeAmplitude = amplitude;
            _shakeDuration = duration;
            _shakeTime = duration;
        }

        public void CancelShake()
        {
            _shakeTime = 0f;
            _shakeDuration = 0f;
            _shakeAmplitude = 0f;
        }

        private Vector3 FollowingPointPosition()
        {
            Vector3 p = _following.position;
            p.y += _offsetY;
            return p;
        }

        private Vector3 GetShakeOffset()
        {
            if (_shakeTime <= 0f)
                return Vector3.zero;

            _shakeTime -= Time.deltaTime;

            float k = _shakeTime / Mathf.Max(0.0001f, _shakeDuration);
            Vector2 r = Random.insideUnitCircle * (_shakeAmplitude * k);

            return new Vector3(r.x, r.y, 0f);
        }
    }
}