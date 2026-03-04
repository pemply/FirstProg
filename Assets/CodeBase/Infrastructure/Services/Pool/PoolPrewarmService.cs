using System;
using CodeBase.Infrastructure.Services.Pool;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class PoolPrewarmService : IService
    {
        private readonly IPoolService _pool;
        private readonly IStaticDataService _static;
        private readonly PoolStaticData _poolStatic;

        public PoolPrewarmService(IPoolService pool, IStaticDataService staticData, PoolStaticData poolStatic)
        {
            _pool = pool;
            _static = staticData;
            _poolStatic = poolStatic;
        }

        public void PrewarmAll()
        {
            // ✅ щоб після restart сцени не тягнути хвости з DontDestroyOnLoad([Pool])
            _pool.Clear();

            PrewarmMonsters();
            PrewarmCommon();
            // PrewarmProjectiles(); // додамо коли покажеш звідки брати projectile prefabs
        }

        private void PrewarmMonsters()
        {
            foreach (MonsterTypeId type in Enum.GetValues(typeof(MonsterTypeId)))
            {
                MonsterStaticData data = _static.ForMonster(type);
                if (data == null || data.PrefabReference == null)
                    continue;

                int count = GetMonsterCount(type);
                if (count > 0)
                    _pool.Prewarm(data.PrefabReference, count);
            }
        }

        private static int GetMonsterCount(MonsterTypeId type) =>
            type switch
            {
                MonsterTypeId.miliEnemy => 30,
                MonsterTypeId.Tank => 20,
                MonsterTypeId.Kamikadze => 20,
                MonsterTypeId.Healer => 20,
                MonsterTypeId.Ranger => 20,
                _ => 30
            };

        private void PrewarmCommon()
        {
            if (_poolStatic == null)
                return;

            // UI popups — тільки якщо реально є prefab
            if (_poolStatic.DamagePopupPrefab != null)
                _pool.Prewarm(_poolStatic.DamagePopupPrefab, 60);

            if (_poolStatic.XpOrbPrefab != null)
                _pool.Prewarm(_poolStatic.XpOrbPrefab, 250);

            if (_poolStatic.AoETelegraphPrefab != null)
                _pool.Prewarm(_poolStatic.AoETelegraphPrefab, 20);

            if (_poolStatic.ProjectileImpactPrefab != null)
                _pool.Prewarm(_poolStatic.ProjectileImpactPrefab, 40);

            if (_poolStatic.ProjectileMuzzlePrefab != null)
                _pool.Prewarm(_poolStatic.ProjectileMuzzlePrefab, 40);
            
            if (_poolStatic.SwordSlashFxPrefab != null)
                _pool.Prewarm(_poolStatic.SwordSlashFxPrefab, 20);
            
            if (_poolStatic.KamikazeExplosionPrefab != null)
                _pool.Prewarm(_poolStatic.KamikazeExplosionPrefab, 20);
        }
    }
}