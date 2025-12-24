using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAttack : MonoBehaviour
    {
        [Header("Attack")]
        public float Damage;
        public float AttackCooldown = 1f;

        private float _cooldownLeft;
        private bool _attackIsActive;
        private IHealth _target;

        private void Update()
        {
            if (!_attackIsActive || _target == null)
                return;

            if (_cooldownLeft > 0)
            {
                _cooldownLeft -= Time.deltaTime;
                return;
            }

            PerformAttack();
        }

        private void PerformAttack()
        {
            _target.TakeDamage(Damage);
            _cooldownLeft = AttackCooldown;
        }

        public void SetTarget(IHealth target)
        {
            _target = target;
        }

        public void ClearTarget()
        {
            _target = null;
        }

        public void EnableAttack() => _attackIsActive = true;
        public void DisableAttack() => _attackIsActive = false;
    }
}