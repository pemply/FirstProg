using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyAnimEvents : MonoBehaviour
    {
        private EnemyAttack _melee;
        private EnemyAreaAttack _area;

        private void Awake()
        {
            _melee = GetComponentInParent<EnemyAttack>(true);
            _area  = GetComponentInParent<EnemyAreaAttack>(true);
        }

        // melee атака
        public void OnAttack()
        {
            _melee?.OnAttack();
        }

        public void OnAttackEnded()
        {
            _melee?.OnAttackEnded();
        }

        // 🔥 кидок гранати
        public void OnThrow()
        {
            Debug.Log("[AnimEvent] THROW → AreaAttack");
            _area?.OnThrowEvent();
        }
    }
}