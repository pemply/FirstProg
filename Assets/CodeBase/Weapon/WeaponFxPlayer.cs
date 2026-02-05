using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Weapon
{
    public class WeaponFxPlayer : MonoBehaviour, IWeaponConfigReceiver
    {
        private WeaponConfig _cfg;

        public void SetConfig(WeaponConfig cfg) => _cfg = cfg;

        public void PlayAttackFx(Vector3 origin)
        {
            if (_cfg == null) return;
            if (_cfg.AttackFxPrefab == null) return;

            // Aura one-shot не потрібен
            if (_cfg.BaseStats.Shape == WeaponStats.AttackShape.Aura)
                return;

            Vector3 pos = origin + _cfg.AttackFxOffset;
            var fx = Instantiate(_cfg.AttackFxPrefab, pos, Quaternion.identity);

            foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>(true))
                ps.Play(true);

            Destroy(fx, _cfg.AttackFxLifetime);
        }
    }
}