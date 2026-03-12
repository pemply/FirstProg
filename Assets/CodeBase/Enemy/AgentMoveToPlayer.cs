using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class AgentMoveToPlayer : MonoBehaviour
    {
        private const float MinStopDistance = 0.1f;
        private const float MinAttackValue = 0.05f;
        private const float NavMeshSampleRadius = 2f;
        private const float DefaultHeroRadius = 0.3f;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [field: SerializeField] public NavMeshAgent Agent { get; private set; }

        [SerializeField] private EnemyAttack _attack;
        [SerializeField] private Animator _animator;
        [SerializeField] private EnemySeparation _separation;

        [Header("Movement")]
        [SerializeField] private float _stopPadding = 0.5f;
        [SerializeField] private float _moveTargetDistance = 1.25f;

        private Transform _heroTransform;
        private CharacterController _heroController;
        private float _heroRadius = DefaultHeroRadius;

        private void Awake()
        {
            CacheRefs();
            _separation = FindFirstObjectByType<EnemySeparation>();
        }

        private void OnEnable()
        {
            if (Agent == null)
                Agent = GetComponent<NavMeshAgent>();

            if (Agent == null)
                return;

            Agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            Agent.avoidancePriority = Random.Range(20, 80);

            if (!Agent.enabled)
                Agent.enabled = true;

            Agent.isStopped = false;
            Agent.ResetPath();

            TryWarpToNavMesh();
        }

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
            _heroController = null;

            if (_heroTransform != null)
            {
                _heroController = _heroTransform.GetComponent<CharacterController>() ??
                                  _heroTransform.GetComponentInParent<CharacterController>();
            }

            _heroRadius = _heroController != null ? _heroController.radius : DefaultHeroRadius;
        }

        private void Update()
        {
            if (!CanMove())
                return;

            if (IsAttacking())
            {
                StopAgent();
                SetSpeed(0f);
                return;
            }

            float stopDistance = CalculateStopDistance();
            float distanceToHero = FlatDistance(transform.position, _heroTransform.position);
            bool shouldMove = distanceToHero > stopDistance;

            if (shouldMove)
                MoveTowardsHero();
            else
                StopAgent();

            UpdateAnimatorSpeed(shouldMove);
        }

        private void CacheRefs()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            if (_attack == null)
                _attack = GetComponent<EnemyAttack>();

            if (_separation == null)
                _separation = GetComponent<EnemySeparation>();

            if (Agent == null)
                Agent = GetComponent<NavMeshAgent>();
        }

        private bool CanMove()
        {
            if (_heroTransform == null)
                return false;

            if (Agent == null || !Agent.enabled)
                return false;

            if (Agent.isOnNavMesh)
                return true;

            return TryWarpToNavMesh();
        }

        private bool TryWarpToNavMesh()
        {
            if (Agent == null)
                return false;

            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
            {
                Agent.Warp(hit.position);
                return true;
            }

            return false;
        }

        private bool IsAttacking() =>
            _attack != null && _attack.IsAttacking;

        private float CalculateStopDistance()
        {
            if (_attack == null || Agent == null)
                return MinStopDistance;

            float enemyRadius = Mathf.Max(0f, Agent.radius);
            float reach = Mathf.Max(MinAttackValue, _attack.EffectiveDistance);
            float hitRadius = Mathf.Max(MinAttackValue, _attack.Cleavage);

            float stopDistance = reach + enemyRadius + _heroRadius - hitRadius + _stopPadding;
            return Mathf.Max(MinStopDistance, stopDistance);
        }

        private void MoveTowardsHero()
        {
            Vector3 toHero = _heroTransform.position - transform.position;
            toHero.y = 0f;
            if (toHero.sqrMagnitude > 0.0001f)
                toHero.Normalize();

            Vector3 offset = _separation != null
                ? _separation.GetMoveOffset(transform.root, transform.position, _heroTransform.position)
                : Vector3.zero;
            Debug.DrawRay(transform.position + Vector3.up * 0.2f, toHero * 1.5f, Color.blue, 0f, false);
            Debug.DrawRay(transform.position + Vector3.up * 0.25f, offset * 2f, Color.red, 0f, false);
            
            
            Agent.isStopped = false;

            Vector3 moveDirection = GetMoveDirection();
            if (moveDirection.sqrMagnitude < 0.0001f)
                moveDirection = transform.forward;

            Vector3 desiredTarget = transform.position + moveDirection * _moveTargetDistance;

            if (NavMesh.SamplePosition(desiredTarget, out NavMeshHit hit, NavMeshSampleRadius, NavMesh.AllAreas))
                Agent.SetDestination(hit.position);
            else
                Agent.SetDestination(desiredTarget);
        }

        private Vector3 GetMoveDirection()
        {
            Vector3 toHero = _heroTransform.position - transform.position;
            toHero.y = 0f;

            if (toHero.sqrMagnitude > 0.0001f)
                toHero.Normalize();

            Vector3 offset = _separation != null
                ? _separation.GetMoveOffset(transform.root, transform.position, _heroTransform.position)
                : Vector3.zero;

            Vector3 jitter = Random.insideUnitSphere * 0.05f;
            jitter.y = 0f;

            Vector3 result = toHero + offset + jitter;
            result.y = 0f;

            return result.sqrMagnitude > 0.0001f ? result.normalized : Vector3.zero;
        }

        private void StopAgent()
        {
            if (Agent == null)
                return;
            Agent.isStopped = true;
            
            Agent.velocity = Vector3.zero;
        }

        private void UpdateAnimatorSpeed(bool isMoving)
        {
            if (_animator == null)
                return;

            float speed01 = 0f;

            if (isMoving && Agent != null && Agent.speed > 0.001f)
                speed01 = Mathf.Clamp01(Agent.velocity.magnitude / Agent.speed);

            _animator.SetFloat(SpeedHash, speed01);
        }

        private void SetSpeed(float value)
        {
            if (_animator != null)
                _animator.SetFloat(SpeedHash, value);
        }

        private static float FlatDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Distance(a, b);
        }
    }
}