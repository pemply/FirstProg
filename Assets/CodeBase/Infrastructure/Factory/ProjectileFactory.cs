using System;
using CodeBase.Combat;
using CodeBase.Hero;
using CodeBase.StaticData;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.Factory
{
    public class ProjectileFactory
    {
        private readonly IStaticDataService _staticData;
        private readonly int _enemyMask;

        // повертає DamageRoll(damage, isCrit)
        public Func<float, DamageRoll> DamageModifier;

        public ProjectileFactory(IStaticDataService staticData)
        {
            _staticData = staticData;
            _enemyMask = LayerMask.GetMask("Enemy");
        }

        public void Spawn(WeaponId weaponId, Vector3 origin, Vector3 dir, WeaponStats stats)
        {
            var cfg = _staticData.GetWeapon(weaponId);
            if (cfg == null || cfg.ProjectilePrefab == null)
                return;

            var go = Object.Instantiate(cfg.ProjectilePrefab, origin, Quaternion.LookRotation(dir, Vector3.up));

            var proj = go.GetComponent<Projectile>();
            if (proj == null)
                return;

            // ---- roll damage (+crit) ----
            DamageRoll roll = DamageModifier != null
                ? DamageModifier(stats.Damage)
                : new DamageRoll(stats.Damage, false);

            // ВАЖЛИВО: прокидаємо isCrit у projectile, щоб він показав попап при попаданні
            proj.SetCrit(roll.IsCrit);

            proj.Construct(
                roll.Damage,
                stats.Range,
                cfg.ProjectileSpeed,
                _enemyMask,
                stats.Pierce
            );
        }
    }
}