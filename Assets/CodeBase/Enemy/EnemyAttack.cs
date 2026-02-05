using System.Linq;
using CodeBase.Logic;
using UnityEngine;
using UnityEngine.AI;
namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [Header("Attack")] public float Cleavage = 0.5f;
        public bool IsAttacking => _isAttacking;

        public float AttackColdown = 3f;
        public float EffectiveDistance = 0.5f;
        public float Damage = 10f;
        private IHealth _heroHealth;
        private NavMeshAgent _agent;
        private EnemyAnimator _anim;
        private Transform _heroTransform;
        private float _attackCooldown;
        private bool _isAttacking;
        private int _layerMask;
        private Collider[] _hits = new Collider[1];
        private bool _attackIsActive;

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
            _heroHealth = heroTransform != null ? heroTransform.GetComponentInParent<IHealth>() : null;
            Debug.Log($"[EnemyAttack] Construct hero={(_heroTransform ? _heroTransform.name : "NULL")} health={(_heroHealth!=null)}");
        }


        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();

            _layerMask = 1 << LayerMask.NameToLayer("Player");
            _anim = GetComponent<EnemyAnimator>();
        }

        private void Update()
        {
            UpdateCooldown();

            if (CanAttack())
                StartAttack();
        }



        private bool CanAttack()
        {
            if (!_attackIsActive || _isAttacking || !CooldownIsUp()) return false;
            if (_heroTransform == null) return false;

            float dist = Vector3.Distance(transform.position, _heroTransform.position);
            return dist <= (EffectiveDistance + Cleavage + 0.2f);
        }

        private bool Hit(out Collider hit)
        {
            var hitCount = Physics.OverlapSphereNonAlloc(StartPoint(), Cleavage, _hits, _layerMask);

            hit = _hits.FirstOrDefault();

            return hitCount > 0;
        }

        private Vector3 StartPoint()
        {
            return new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z) +
                   transform.forward * EffectiveDistance;
        }


        private void UpdateCooldown()
        {
            if (!CooldownIsUp())
            {
                _attackCooldown -= Time.deltaTime;
            }
        }

        private bool CooldownIsUp()
        {
            return _attackCooldown <= 0;
        }

        // ✅ ЦЕ ТЕПЕР викликає Animation Event у кліпі атаки
        public void OnAttack()
        {
            if (_heroTransform == null || _heroHealth == null)
                return;

            // перевірка “чи дістаю” — використовуємо твої параметри (з SO)
            float dist = Vector3.Distance(StartPoint(), _heroTransform.position);
            if (dist > (Cleavage + 0.2f)) return;

            _heroHealth.TakeDamage(Damage);
        }


        // ✅ теж через Animation Event в кінці кліпу
        public void OnAttackEnded()
        {
            _attackCooldown = AttackColdown;
            _isAttacking = false;
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.isStopped = false;

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
                _agent.ResetPath(); // важливо, щоб velocity стало 0
            }
            _isAttacking = true;

            // ✅ тільки запускаємо анімацію
            _anim?.PlayAttack();
        }


        public void EnableAttack()
        {
            _attackIsActive = true;
        }

        public void DisableAttack()
        {
            _attackIsActive = false;
        }
    }
}