using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class AgentMoveToPlayer : MonoBehaviour
    {
        public NavMeshAgent Agent;

        [SerializeField] private EnemyAttack _attack;
        [SerializeField] private Animator _animator;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");

        [Header("Tuning")]
        [SerializeField] private float _stopPadding = .5f; // маленький запас, щоб не "стопало зарано"

        private Transform _heroTransform;
        private CharacterController _heroCC;
        private float _heroRadius = 0.3f; // fallback

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            if (_attack == null)
                _attack = GetComponent<EnemyAttack>();
        }

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;

            _heroCC = null;
            if (heroTransform != null)
                _heroCC = heroTransform.GetComponent<CharacterController>() ?? heroTransform.GetComponentInParent<CharacterController>();

            _heroRadius = _heroCC != null ? _heroCC.radius : 0.3f;
        }

        private void Update()
        {
            if (_heroTransform == null) return;
            if (Agent == null || !Agent.enabled || !Agent.isOnNavMesh) return;

            // якщо зараз атакує — не рухаємось
            if (_attack != null && _attack.IsAttacking)
            {
                StopAgent();
                SetSpeed(0f);
                return;
            }

            float stopDist = 0.1f;

            if (_attack != null)
            {
                float enemyR = Mathf.Max(0f, Agent.radius);

                // EffectiveDistance — де стоїть "точка удару" від ворога
                float reach = Mathf.Max(0.05f, _attack.EffectiveDistance);

                // Cleavage — радіус сфери удару (OverlapSphere)
                float hitR = Mathf.Max(0.05f, _attack.Cleavage);

                // ✅ ключ:
                // на дистанції stopDist герой вже має потрапляти в OverlapSphere біля StartPoint.
                // Тому НЕ можна стопати на reach + радіуси (це зарано).
                // Треба стопати ближче: віднімаємо hitR.
                stopDist = reach + enemyR + _heroRadius - hitR + _stopPadding;

                // safety
                stopDist = Mathf.Max(0.1f, stopDist);
            }

            // dist в XZ
            Vector3 a = Agent.transform.position; a.y = 0f;
            Vector3 b = _heroTransform.position;  b.y = 0f;
            float dist = Vector3.Distance(a, b);

            bool move = dist > stopDist;

            if (move)
            {
                Vector3 target = _heroTransform.position;

                if (NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas))
                    Agent.SetDestination(hit.position);
                else
                    Agent.SetDestination(target);
            }
            else
            {
                StopAgent();
            }

            float speed01 = (Agent.speed <= 0.001f) ? 0f : Mathf.Clamp01(Agent.velocity.magnitude / Agent.speed);
            SetSpeed(move ? speed01 : 0f);
        }

        private void StopAgent()
        {
            if (Agent.hasPath)
                Agent.ResetPath();

            Agent.velocity = Vector3.zero;
        }

        private void SetSpeed(float v)
        {
            if (_animator != null)
                _animator.SetFloat(SpeedHash, v);
        }
    }
}
