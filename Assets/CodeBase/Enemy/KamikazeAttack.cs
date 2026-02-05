using System.Linq;
using CodeBase.Logic;
using UnityEngine;


namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class KamikazeAttack : MonoBehaviour
    {
        public float Cleavage = 0.5f;
        public float AttackColdown = 0.5f;
        public float EffectiveDistance = 0.5f;
        public float Damage = 10f;

        private Transform _heroTransform;
        private float _attackCooldown;
        private int _layerMask;
        private readonly Collider[] _hits = new Collider[1];
        private EnemyHealth _selfHealth;

        public void Construct(Transform heroTransform) => _heroTransform = heroTransform;

        private void Awake()
        {
            _selfHealth = GetComponent<EnemyHealth>();
            _layerMask = 1 << LayerMask.NameToLayer("Player");
        }

        private void Update()
        {
            if (_heroTransform == null)
                return;

            if (_attackCooldown > 0f)
            {
                _attackCooldown -= Time.deltaTime;
                return;
            }

            transform.LookAt(_heroTransform);

            if (Hit(out Collider hit))
            {
                // 1) герой отримав урон
                hit.GetComponentInParent<IHealth>()?.TakeDamage(Damage);

                // 2) камікадзе вибухає (анімація + VFX + Destroy)
                GetComponent<EnemyAnimator>()?.PlayExplode();

                // 3) щоб не повторив атаку
                enabled = false;
            }
        }

        private bool Hit(out Collider hit)
        {
            int hitCount = Physics.OverlapSphereNonAlloc(StartPoint(), Cleavage, _hits, _layerMask);
            hit = _hits.FirstOrDefault();
            return hitCount > 0;
        }

        private Vector3 StartPoint()
        {
            return new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z) +
                   transform.forward * EffectiveDistance;
        }
    }
}