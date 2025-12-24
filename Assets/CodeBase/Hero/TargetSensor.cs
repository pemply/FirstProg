using System.Collections.Generic;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Logic
{
    [RequireComponent(typeof(SphereCollider))]
    public class TargetSensor : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;

        private SphereCollider _collider;
        private readonly HashSet<IHealth> _targets = new HashSet<IHealth>();

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
        }

        public void SetRadius(float radius) =>
            _collider.radius = radius;

        public bool TryGetNearest(Vector3 from, out IHealth nearest)
        {
            nearest = null;
            float bestSqr = float.PositiveInfinity;

            // санітарна чистка null-ів (ворог міг Destroy)
            _targets.RemoveWhere(t => t == null);

            foreach (IHealth t in _targets)
            {
                // IHealth може бути компонентом на GameObject
                var mb = t as MonoBehaviour;
                if (mb == null) 
                    continue;

                float sqr = (mb.transform.position - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = t;
                }
            }

            return nearest != null;
        }

        private void OnTriggerEnter(Collider other)
        {

            if (((1 << other.gameObject.layer) & _targetMask) == 0)
                return;

            bool ok = other.TryGetComponent(out IHealth h1);
            IHealth h2 = other.GetComponentInParent<IHealth>();
            
            if (!other.TryGetComponent(out IHealth health))
                health = other.GetComponentInParent<IHealth>();

            if (health != null)
                _targets.Add(health);
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & _targetMask) == 0)
                return;

            if (!other.TryGetComponent(out IHealth health))
                health = other.GetComponentInParent<IHealth>();

            if (health != null)
                _targets.Remove(health);
        }
    }
}
