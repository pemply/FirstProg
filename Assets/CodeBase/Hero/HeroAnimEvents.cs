using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroAnimEvents : MonoBehaviour
    {
        private WeaponAttackRunner _primary;

        private void Awake()
        {
            var runners = transform.root.GetComponentsInChildren<WeaponAttackRunner>(true);

            for (int i = 0; i < runners.Length; i++)
            {
                if (runners[i].IsPrimarySlot)
                {
                    _primary = runners[i];
                    return;
                }
            }

            Debug.LogError("[HeroAnimEvents] Primary runner not found!");
        }

        public void OnAttack() => _primary?.OnAttack();
        public void OnAttackEnded() => _primary?.OnAttackEnded();
    }
}