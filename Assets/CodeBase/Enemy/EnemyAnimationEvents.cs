using UnityEngine;

namespace CodeBase.Enemy
{
    // має висіти НА ТОМУ Ж GO, ДЕ ANIMATOR
    public class EnemyAnimEvents : MonoBehaviour
    {
        private EnemyAttack _attack;

        private void Awake()
        {
            _attack = GetComponentInParent<EnemyAttack>(true);
            if (_attack == null)
                Debug.LogError("[EnemyAnimEvents] EnemyAttack not found in parents", this);
        }

        // Назву зроби такою ж як у Animation Event:
        public void OnAttack()
        {
            _attack?.OnAttack();
        }

        public void OnAttackEnded()
        {
            _attack?.OnAttackEnded();
        }
    }
}