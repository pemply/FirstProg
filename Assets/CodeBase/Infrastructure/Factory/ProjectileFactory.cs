using CodeBase.Infrastructure.AssetManagement;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class ProjectileFactory
    {
        private readonly IStaticDataService _staticData;
        private readonly int _enemyMask;

        public ProjectileFactory(  IStaticDataService staticData)
        {
            _staticData = staticData;
            _enemyMask = LayerMask.GetMask("Enemy");
        }

        public void Spawn(WeaponId weaponId, Vector3 origin, Vector3 dir, WeaponStats stats)
        {

            var cfg = _staticData.GetWeapon(weaponId);
            if (cfg == null || cfg.ProjectilePrefab == null)
                return;

            // Якщо ProjectilePrefab addressable — краще через _assets.Instantiate(key)
            // Але ти зараз тримаєш GameObject, тому просто Instantiate:
            var go = Object.Instantiate(cfg.ProjectilePrefab, origin, Quaternion.LookRotation(dir, Vector3.up));

            var proj = go.GetComponent<Projectile>();
            if (proj == null)
            {
                
                return;
            } 

            proj.Construct(
                stats.Damage,
                stats.Range,
                cfg.ProjectileSpeed,
                _enemyMask,
                stats.Pierce
            );
        }
    }
}