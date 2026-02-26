using System.Collections.Generic;
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

        // ✅ кеш параметрів аніматора, щоб не було "Hash ... does not exist"
        private HashSet<int> _animParams;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);

            if (_attack == null)
                _attack = GetComponent<EnemyAttack>();

            if (Agent == null)
                Agent = GetComponent<NavMeshAgent>();

            CacheAnimatorParams();
        }

        private void OnEnable()
        {
            if (Agent == null)
                Agent = GetComponent<NavMeshAgent>();

            if (Agent == null) return;

            // ✅ важливо для пула: скинути стан з минулого життя
            if (!Agent.enabled)
                Agent.enabled = true;

            Agent.isStopped = false;
            Agent.ResetPath();

            // ✅ якщо виліз трохи повз NavMesh — варп на найближчу точку
            if (!Agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
                    Agent.Warp(hit.position);
            }
        }

        private void CacheAnimatorParams()
        {
            if (_animator == null)
            {
                _animParams = null;
                return;
            }

            var ps = _animator.parameters;
            _animParams = new HashSet<int>(ps != null ? ps.Length : 0);

            if (ps != null)
            {
                for (int i = 0; i < ps.Length; i++)
                    _animParams.Add(ps[i].nameHash);
            }
        }

        private bool HasParam(int hash) =>
            _animator != null && _animParams != null && _animParams.Contains(hash);

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;

            _heroCC = null;
            if (heroTransform != null)
                _heroCC = heroTransform.GetComponent<CharacterController>() ??
                          heroTransform.GetComponentInParent<CharacterController>();

            _heroRadius = _heroCC != null ? _heroCC.radius : 0.3f;
        }

        private void Update()
        {
            if (_heroTransform == null) return;
            if (Agent == null || !Agent.enabled) return;

            // ✅ якщо з якихось причин агент не на NavMesh (пул/спавн) — спробувати виправити
            if (!Agent.isOnNavMesh)
            {
                if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
                    Agent.Warp(hit.position);
                else
                    return;
            }

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

                float reach = Mathf.Max(0.05f, _attack.EffectiveDistance);
                float hitR  = Mathf.Max(0.05f, _attack.Cleavage);

                stopDist = reach + enemyR + _heroRadius - hitR + _stopPadding;
                stopDist = Mathf.Max(0.1f, stopDist);
            }

            Vector3 a = Agent.transform.position; a.y = 0f;
            Vector3 b = _heroTransform.position;  b.y = 0f;
            float dist = Vector3.Distance(a, b);

            bool move = dist > stopDist;

            if (move)
            {
                // ✅ важливо: якщо десь раніше його стопнули (смерть/стун/атака) — розстопорити
                Agent.isStopped = false;

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
            if (Agent == null) return;

            // ✅ щоб не було "завис у stopped=false/true" станах
            Agent.isStopped = true;

            if (Agent.hasPath)
                Agent.ResetPath();

            Agent.velocity = Vector3.zero;
        }

        private void SetSpeed(float v)
        {
            // ✅ якщо нема параметра Speed — просто мовчки нічого не робимо
            if (!HasParam(SpeedHash))
                return;

            _animator.SetFloat(SpeedHash, v);
        }
    }
}