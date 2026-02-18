using UnityEngine;

namespace CodeBase.Enemy
{
    public class AttackSensorRadiusApplier : MonoBehaviour
    {
        [SerializeField] private TriggerObserver _observer;
      

        private SphereCollider _sensor;

        public void Apply(float effectiveDistance, float cleavage)
        {
            if (_observer == null)
                _observer = GetComponentInChildren<TriggerObserver>(true);

            if (_observer == null)
            {
                Debug.LogWarning($"[AttackSensorRadiusApplier] TriggerObserver missing on {name}", this);
                return;
            }

            _sensor = _observer.GetComponent<SphereCollider>();
            if (_sensor == null)
                _sensor = _observer.GetComponentInChildren<SphereCollider>(true);

            if (_sensor == null)
            {
                Debug.LogWarning($"[AttackSensorRadiusApplier] SphereCollider missing under '{_observer.name}'", _observer);
                return;
            }

            _sensor.isTrigger = true;
            _sensor.radius = Mathf.Max(0.05f, effectiveDistance + cleavage);
        }
    }
}