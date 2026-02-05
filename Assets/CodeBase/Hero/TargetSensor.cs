using System.Collections.Generic;
using CodeBase.Enemy;
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

            // ✅ чистимо null і dead
            _targets.RemoveWhere(t =>
            {
                if (t == null) return true;
                if (t is EnemyHealth eh && eh.IsDead) return true;
                return false;
            });

            foreach (IHealth t in _targets)
            {
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

            IHealth health = other.GetComponentInParent<IHealth>();
            if (health == null) return;

            if (health is EnemyHealth eh && eh.IsDead)
                return;

            _targets.Add(health);
        }


        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & _targetMask) == 0)
                return;

            IHealth health = other.GetComponentInParent<IHealth>();
            if (health == null) return;

            _targets.Remove(health);
        }

    }
}
