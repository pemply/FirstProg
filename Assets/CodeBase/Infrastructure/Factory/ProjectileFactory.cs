using System;
using CodeBase.Hero;
using CodeBase.StaticData;
using CodeBase.Infrastructure.Services.Pool;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class ProjectileFactory
    {
        private readonly IStaticDataService _staticData;
        private readonly IPoolService _pool;
        private readonly int _enemyMask;

        public Func<float, DamageRoll> DamageModifier;
        private IDamagePopupService _damagePopups;

        public ProjectileFactory(IStaticDataService staticData, IPoolService pool)
        {
            _staticData = staticData;
            _pool = pool;
            _enemyMask = LayerMask.GetMask("Enemy");
        }

        public void SetDamagePopups(IDamagePopupService damagePopups) => _damagePopups = damagePopups;

        public void Spawn(WeaponId weaponId, Vector3 origin, Vector3 dir, WeaponStats stats)
        {
            var cfg = _staticData.GetWeapon(weaponId);
            if (cfg == null || cfg.ProjectilePrefab == null)
                return;

            var rot = Quaternion.LookRotation(dir, Vector3.up);

            // ✅ пул
            var go = _pool.Get(cfg.ProjectilePrefab, origin, rot);

            var proj = go.GetComponent<Projectile>();
            if (proj == null)
                return;

            DamageRoll roll = DamageModifier != null
                ? DamageModifier(stats.Damage)
                : new DamageRoll(stats.Damage, false);

            proj.SetCrit(roll.IsCrit);

            proj.Construct(
                roll.Damage,
                stats.Range,
                cfg.ProjectileSpeed,
                _enemyMask,
                stats.Pierce,
                _damagePopups
            );
        }
    }
}