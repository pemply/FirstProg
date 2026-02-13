using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class AgentMoveToPlayer : MonoBehaviour
    {
        private const float MinimalDistance = 1f;

        public NavMeshAgent Agent;
        [SerializeField] private EnemyAttack _attack;
        [SerializeField] private Animator _animator;
        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        private Transform _heroTransform;
        private float _logTimer;

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>(true);
            _attack = GetComponent<EnemyAttack>();
        }

        public void Construct(Transform heroTransform) => _heroTransform = heroTransform;

        private void Update()
        {
            if (_heroTransform == null) return;
            if (Agent == null || !Agent.enabled || !Agent.isOnNavMesh) return;
            if (_attack != null && _attack.IsAttacking)
            {
                _animator.SetFloat(SpeedHash, 0f);
                return;
            }

            var attack = GetComponent<EnemyAttack>();

            if (attack != null && attack.IsAttacking)
            {
                _animator.SetFloat(SpeedHash, 0f);
                return;
            }

            float dist = Vector3.Distance(Agent.transform.position, _heroTransform.position);
            bool move = dist > MinimalDistance;

            if (move)
                Agent.SetDestination(_heroTransform.position);

            // speed 0..1
            float speed01 = (Agent.speed <= 0.001f) ? 0f : Mathf.Clamp01(Agent.velocity.magnitude / Agent.speed);
            _logTimer += Time.deltaTime;
            if (_logTimer >= 0.5f)
            {
                _logTimer = 0f;
            }

            // SET PARAM
            if (_animator != null)
                _animator.SetFloat(SpeedHash, move ? speed01 : 0f);

            // DEBUG раз на ~0.5с
            _logTimer += Time.deltaTime;
            if (_logTimer >= 0.5f)
            {
                _logTimer = 0f;
            }
        }

        private static bool HasParam(Animator a, string name)
        {
            if (a == null) return false;
            foreach (var p in a.parameters)
                if (p.name == name) return true;
            return false;
        }
    }
}
