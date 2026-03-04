using CodeBase.GameLogic;
using CodeBase.Logic;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour, IEnemyAttack
    {
        [Header("Attack")]
        public float Cleavage = 0.5f;
        public float AttackColdown = 3f;
        public float EffectiveDistance = 0.5f;
        public float Damage = 10f;

        [Header("Hit Filter")]
        [SerializeField] private LayerMask _heroMask; // ✅ постав HeroHurtbox

        public bool IsAttacking => _isAttacking;

        private IHealth _heroHealth;
        private Transform _heroTransform;

        private NavMeshAgent _agent;
        private EnemyAnimator _anim;

        private float _attackCooldown;
        private bool _isAttacking;
        private bool _attackIsActive;

        private float _heroCenterY = 1.0f;
        private readonly Collider[] _hits = new Collider[8];

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
            _heroHealth = heroTransform != null ? heroTransform.GetComponentInParent<IHealth>() : null;

            if (heroTransform != null)
            {
                var cc = heroTransform.GetComponent<CharacterController>()
                         ?? heroTransform.GetComponentInParent<CharacterController>();

                if (cc != null)
                    _heroCenterY = cc.center.y;
            }
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _anim = GetComponent<EnemyAnimator>();
        }

        private void Update()
        {
            if (_attackCooldown > 0f)
                _attackCooldown -= Time.deltaTime;

            if (CanAttack())
                StartAttack();
        }

        private bool CanAttack()
        {
            if (!_attackIsActive) return false;
            if (_isAttacking) return false;
            if (_attackCooldown > 0f) return false;
            if (_heroTransform == null || _heroHealth == null) return false;

            return HitHero(); // ✅ одна логіка
        }

        // Animation Event
        public void OnAttack()
        {
            if (_heroHealth == null || _heroTransform == null)
                return;

            if (HitHero())
                _heroHealth.TakeDamage(Damage);
        }

        // Animation Event
        public void OnAttackEnded()
        {
            _attackCooldown = AttackColdown;
            _isAttacking = false;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.isStopped = false;
        }

        private bool HitHero()
        {
            Vector3 p = StartPoint();

            int hitCount = Physics.OverlapSphereNonAlloc(
                p,
                Cleavage,
                _hits,
                _heroMask,
                QueryTriggerInteraction.Ignore); // ✅ ігноримо trigger-и

            for (int i = 0; i < hitCount; i++)
            {
                Collider c = _hits[i];
                if (c == null) continue;

                // hurtbox — дитина героя, тому IHealth знайдеться в parent
                if (c.GetComponentInParent<IHealth>() == _heroHealth)
                    return true;
            }

            return false;
        }

        private Vector3 StartPoint()
        {
            Vector3 p = transform.position;

            Vector3 fwd = transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < Constant.Epsilone) fwd = Vector3.forward;
            fwd.Normalize();

            p += fwd * EffectiveDistance;

            if (_heroTransform != null)
                p.y = _heroTransform.position.y + _heroCenterY;
            else
                p.y += 1.0f;

            return p;
        }

        private void StartAttack()
        {
            if (_heroTransform == null)
                return;

            Vector3 dir = _heroTransform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.ResetPath();
            }

            _isAttacking = true;
            _anim?.PlayAttack();
        }

        public void EnableAttack() => _attackIsActive = true;
        public void DisableAttack() => _attackIsActive = false;

        public void ResetForReuse()
        {
            _attackCooldown = 0f;
            _isAttacking = false;
            _attackIsActive = false;
            enabled = true;

            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.isStopped = false;
        }
    }
}