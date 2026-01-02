using System.Linq;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [Header("Attack")]


        public float Cleavage = 0.5f;

        public float AttackColdown = 3f;
        public float EffectiveDistance = 0.5f;
        public float Damage = 10f;


        private Transform _heroTransform;
        private float _attackCooldown;
        private bool _isAttacking;
        private int _layerMask;
        private Collider[] _hits = new Collider[1];
        private bool _attackIsActive;

        public void Construct(Transform heroTransform)
        {
            _heroTransform = heroTransform;
        }

        private void Awake()
        {

            _layerMask = 1 << LayerMask.NameToLayer("Player");
         
        }

        private void Update()
        {
            UpdateCooldown();
            if (CanAttack())
                StartAttack();
        }

        private bool CanAttack() =>
           _attackIsActive && !_isAttacking && CooldownIsUp();

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

        public void OnAttack()
        {
            if (Hit(out Collider hit))
            {
 
                var health = hit.GetComponentInParent<IHealth>();
                if (health != null)
                    health.TakeDamage(Damage);

            }
        }

        public void OnAttackEnded()
        {
            _attackCooldown = AttackColdown;
            _isAttacking = false;
        }

        private void StartAttack()
        {
            if (_heroTransform == null)
                return;

            transform.LookAt(_heroTransform);

            _isAttacking = true;

            OnAttack();      // <-- наносимо дамаг
            OnAttackEnded(); // <-- ставимо кулдаун і дозволяємо наступну атаку
        }

        
        public void EnableAttack()
        {
          _attackIsActive  = true;
        }

        public void DisableAttack()
        {
          _attackIsActive =  false;
        }
    }
}