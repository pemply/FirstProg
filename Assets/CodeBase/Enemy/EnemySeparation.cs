using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemySeparation : MonoBehaviour
    {
        [Header("Separation")]
        [SerializeField] private float _separationRadius = 2f;
        [SerializeField] private float _separationStrength = 3f;
        [SerializeField] private int _maxHits = 8;
        private LayerMask _enemyMask;

        [Header("Orbit")]
        [SerializeField] private float _orbitDistance = 3f;
        [SerializeField] private float _orbitStrength = 2f;

        private Collider[] _hits;

        private void Awake()
        {
            if (_enemyMask.value == 0)
                _enemyMask = LayerMask.GetMask("Enemy");

            _hits = new Collider[Mathf.Max(4, _maxHits)];
        }

        public Vector3 GetMoveOffset(Transform selfRoot, Vector3 selfPosition, Vector3 heroPosition)
        {
            Vector3 separation = GetSeparationDirection(selfRoot, selfPosition);
            Vector3 orbit = GetOrbitOffset(selfRoot, selfPosition, heroPosition);

            Vector3 result = separation * _separationStrength + orbit;
            result.y = 0f;
            return result;
        }

        private Vector3 GetSeparationDirection(Transform selfRoot, Vector3 selfPosition)
        {
            if (_hits == null || _hits.Length == 0)
                return Vector3.zero;

            int count = Physics.OverlapSphereNonAlloc(
                selfPosition,
                _separationRadius,
                _hits,
                _enemyMask,
                QueryTriggerInteraction.Collide);

            if (count <= 1)
                return Vector3.zero;

            selfPosition.y = 0f;
            Vector3 push = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                Collider hit = _hits[i];
                if (hit == null)
                    continue;

                Transform otherRoot = hit.transform.root;
                if (otherRoot == selfRoot)
                    continue;

                Vector3 otherPos = otherRoot.position;
                otherPos.y = 0f;

                Vector3 diff = selfPosition - otherPos;
                float sqrDistance = diff.sqrMagnitude;

                if (sqrDistance < 0.0001f)
                    continue;

                float distance = Mathf.Sqrt(sqrDistance);
                if (distance > _separationRadius)
                    continue;

                float weight = 1f - distance / _separationRadius;
                push += (diff / distance) * weight;
            }

            if (push.sqrMagnitude < 0.0001f)
                return Vector3.zero;

            return push.normalized;
        }

        private Vector3 GetOrbitOffset(Transform selfRoot, Vector3 selfPosition, Vector3 heroPosition)
        {
            Vector3 toHero = heroPosition - selfPosition;
            toHero.y = 0f;

            float distance = toHero.magnitude;
            if (distance > _orbitDistance || distance < 0.001f)
                return Vector3.zero;

            Vector3 dir = toHero / distance;
            Vector3 side = Vector3.Cross(Vector3.up, dir);

            float sign = (selfRoot.GetInstanceID() & 1) == 0 ? 1f : -1f;
            float weight = 1f - distance / _orbitDistance;

            return side * (sign * _orbitStrength * weight);
        }
    }
}