using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent agent;

        [Header("Explosion (optional)")]
        [SerializeField] private GameObject _explosionVfxPrefab;
        [SerializeField] private float _destroyDelayAfterDie = 1.2f;
        [SerializeField] private float _destroyDelayAfterExplode = 0.15f;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int ExplodeHash = Animator.StringToHash("Explode");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private bool _finished;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_finished)
                return;

            if (!IsAgentValid())
            {
                _animator.SetBool(IsMovingHash, false);
                return;
            }

            bool moving =
                agent.remainingDistance > agent.stoppingDistance + 0.05f &&
                agent.velocity.sqrMagnitude > 0.01f;

            _animator.SetBool(IsMovingHash, moving);
        }

        private bool IsAgentValid()
        {
            return agent != null
                   && agent.isActiveAndEnabled
                   && agent.gameObject.activeInHierarchy
                   && agent.isOnNavMesh;
        }

        public void PlayAttack()
        {
            if (_finished) return;
            _animator.SetTrigger(AttackHash);
        }

        public void PlayDeath()
        {
            if (_finished) return;
            _finished = true;

            // щоб точно перестав “рухатись” і не було залишкових рухів
            _animator.SetBool(IsMovingHash, false);

            // опційно: зупиняємо агент одразу
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.enabled = false; // щоб точно не було isOnNavMesh/remainingDistance проблем
            }

            _animator.SetTrigger(DieHash);
            Destroy(transform.root.gameObject, _destroyDelayAfterDie);
        }

        public void PlayExplode()
        {
            if (_finished) return;
            _finished = true;

            _animator.SetBool(IsMovingHash, false);

            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            _animator.SetTrigger(ExplodeHash);

            if (_explosionVfxPrefab != null)
                Instantiate(_explosionVfxPrefab, transform.position, Quaternion.identity);

            Destroy(transform.root.gameObject, _destroyDelayAfterExplode);
        }
    }
}
