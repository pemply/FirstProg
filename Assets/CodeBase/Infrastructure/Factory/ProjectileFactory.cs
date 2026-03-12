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

        public void Spawn(WeaponId weaponId, Vector3 origin, Vector3 dir, WeaponStats stats, Transform ownerRoot)
        {
            
            var cfg = _staticData.GetWeapon(weaponId);
            if (cfg == null || cfg.ProjectilePrefab == null)
                return;

            var rot = Quaternion.LookRotation(dir, Vector3.up);
// muzzle
            if (cfg.ProjectileMuzzlePrefab != null)
            {
              
                _pool.Get(cfg.ProjectileMuzzlePrefab, origin, rot);
            }
            var go = _pool.Get(cfg.ProjectilePrefab, origin, rot);

            var proj = go.GetComponent<Projectile>();
            if (proj == null)
                return;

            DamageRoll roll = DamageModifier != null
                ? DamageModifier(stats.Damage)
                : new DamageRoll(stats.Damage, false);

            proj.SetCrit(roll.IsCrit);

            proj.Construct(
                damage: roll.Damage,
                range: stats.Range,
                speed: cfg.ProjectileSpeed,
                enemyMask: _enemyMask,
                pierce: stats.Pierce,
                knockback: stats.Knockback,
                knockbackChance: stats.KnockbackChance,
                damagePopups: _damagePopups,
                impactFx: cfg.ProjectileImpactPrefab,
                muzzleFx: cfg.ProjectileMuzzlePrefab,
                ownerRoot: ownerRoot,
                pool: _pool
            );
        }
    }
}