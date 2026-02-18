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

            if (_targetMask.value == 0)
                _targetMask = LayerMask.GetMask("Player");
        }

        public void SetRadius(float radius) => _collider.radius = radius;

        private void CleanupTargets()
        {
            _targets.RemoveWhere(t =>
            {
                if (t == null) return true;

                // ✅ головне: прибираємо Destroy() Unity-об'єкти
                var mb = t as MonoBehaviour;
                if (mb == null) return true;

                if (t is EnemyHealth eh && eh.IsDead) return true;

                return false;
            });
        }

        public bool TryGetNearest(Vector3 from, out IHealth nearest)
        {
            nearest = null;
            float bestSqr = float.PositiveInfinity;

            CleanupTargets();

            foreach (IHealth t in _targets)
            {
                var mb = t as MonoBehaviour;
                if (mb == null) continue;

                float sqr = (mb.transform.position - from).sqrMagnitude;
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = t;
                }
            }

            return nearest != null;
        }

        public bool TryGetNearestInFront(Vector3 from, Vector3 forward, float coneAngleDeg, out IHealth nearest)
        {
            nearest = null;
            float bestSqr = float.PositiveInfinity;

            CleanupTargets();

            Vector3 fwd = forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) return false;
            fwd.Normalize();

            float cos = Mathf.Cos((coneAngleDeg * 0.5f) * Mathf.Deg2Rad);

            foreach (IHealth t in _targets)
            {
                var mb = t as MonoBehaviour;
                if (mb == null) continue;

                Vector3 to = mb.transform.position - from;
                to.y = 0f;

                float sqr = to.sqrMagnitude;
                if (sqr < Constant.Epsilone) continue;

                float dot = Vector3.Dot(fwd, to / Mathf.Sqrt(sqr));
                if (dot < cos) continue;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    nearest = t;
                }
            }

            return nearest != null;
        }

        private bool MaskOk(Collider other)
        {
            int layer = other.gameObject.layer;
            if (((1 << layer) & _targetMask.value) != 0)
                return true;

            int rootLayer = other.transform.root.gameObject.layer;
            return ((1 << rootLayer) & _targetMask.value) != 0;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!MaskOk(other))
                return;

            IHealth health = other.GetComponentInParent<IHealth>();
            if (health == null) return;
            if (health is EnemyHealth eh && eh.IsDead) return;

            _targets.Add(health);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!MaskOk(other))
                return;

            IHealth health = other.GetComponentInParent<IHealth>();
            if (health == null) return;

            _targets.Remove(health);
        }
    }
}
