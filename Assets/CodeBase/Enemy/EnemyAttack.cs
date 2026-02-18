using CodeBase.Logic;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour, IEnemyAttack
    {
        [Header("Attack")]
        public float Cleavage = 0.5f;          // радіус удару (сфера)
        public float AttackColdown = 3f;       // кулдаун між атаками
        public float EffectiveDistance = 0.5f; // “довжина руки” вперед до точки удару
        public float Damage = 10f;

        public bool IsAttacking => _isAttacking;

        private IHealth _heroHealth;
        private Transform _heroTransform;

        private NavMeshAgent _agent;
        private EnemyAnimator _anim;

        private float _attackCooldown;
        private bool _isAttacking;
        private bool _attackIsActive;

        // кеш геометрії героя (щоб не GetComponent кожен кадр)
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

            // ✅ герой реально має попасти в зону удару (сфера біля "кулака")
            Vector3 p = StartPoint();

            int hitCount = Physics.OverlapSphereNonAlloc(p, Cleavage, _hits);
            for (int i = 0; i < hitCount; i++)
            {
                Collider c = _hits[i];
                if (c == null) continue;

                if (c.GetComponentInParent<IHealth>() == _heroHealth)
                    return true;
            }

            return false;
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

            int hitCount = Physics.OverlapSphereNonAlloc(p, Cleavage, _hits);
            for (int i = 0; i < hitCount; i++)
            {
                Collider c = _hits[i];
                if (c == null) continue;

                if (c.GetComponentInParent<IHealth>() == _heroHealth)
                    return true;
            }

            return false;
        }

        private Vector3 StartPoint()
        {
            // ✅ точка удару по лінії до героя, щоб не промахувалось, коли агент стоїть боком
            Vector3 p = transform.position;

            if (_heroTransform != null)
            {
                Vector3 to = _heroTransform.position - transform.position;
                to.y = 0f;

                float dist = to.magnitude;
                Vector3 dir = dist > 0.0001f ? (to / dist) : transform.forward;

                float d = Mathf.Min(EffectiveDistance, dist);
                p += dir * d;

                // по висоті центру героя
                p.y = _heroTransform.position.y + _heroCenterY;
            }
            else
            {
                // fallback
                p += transform.forward * EffectiveDistance;
                p.y += 1.0f;
            }

            return p;
        }

        private void StartAttack()
        {
            if (_heroTransform == null)
                return;

            // повертаємось до героя (XZ)
            Vector3 dir = _heroTransform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);

            // стоп агента на час атаки
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
    }
}
