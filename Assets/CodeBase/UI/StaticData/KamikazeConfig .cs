using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Logic.Upgrade;
using CodeBase.StaticData.CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private Dictionary<MonsterTypeId, MonsterStaticData> _monsters;
        private Dictionary<WeaponId, WeaponConfig> _weapons;
        private Dictionary<HeroId, HeroConfig> _heroes;

        private List<UpgradeConfig> _upgrades;
        public IReadOnlyList<UpgradeConfig> AllUpgrades => _upgrades;

        private UpgradeRarityChances _rarityChances;
        public UpgradeRarityChances RarityChances => _rarityChances;
        private Dictionary<MonsterTypeId, KamikazeConfig > _kamikaze;

        public void LoadMonsters()
        {
            _monsters = Resources
                .LoadAll<MonsterStaticData>(AssetsPath.EnemyConfig)
                .ToDictionary(x => x.MonsterTypeId, x => x);
        }
        
        public void LoadWeapons()
        {
            _weapons = new Dictionary<WeaponId, WeaponConfig>();

            foreach (var w in Resources.LoadAll<WeaponConfig>(AssetsPath.WeaponConfig))
            {
                if (w == null) continue;
                _weapons[w.WeaponId] = w;
            }
        }

        public List<WeaponId> AllWeaponIds() =>
            _weapons != null ? _weapons.Keys.ToList() : new List<WeaponId>();

        public void LoadUpgrades()
        {
            _upgrades = Resources.LoadAll<UpgradeConfig>(AssetsPath.UpgradeConfig).ToList();
            _rarityChances = Resources.Load<UpgradeRarityChances>(AssetsPath.ChanceRarityUpg);
        }

        public MonsterStaticData ForMonster(MonsterTypeId typeId) =>
            _monsters != null && _monsters.TryGetValue(typeId, out var sd) ? sd : null;

        public WeaponConfig GetDefaultWeapon()
        {
            if (_weapons == null || _weapons.Count == 0)
            {
                Debug.LogError("[StaticDataService] Weapons not loaded or empty. Check Resources/Weapon");
                return null;
            }

            return _weapons.Values.First();
        }

        public WeaponConfig GetWeapon(WeaponId weaponId)
        {
            if (_weapons != null && _weapons.TryGetValue(weaponId, out var w))
                return w;

            return GetDefaultWeapon();
        }

        private void LoadHeroes()
        {
            var list = Resources.LoadAll<HeroConfig>(AssetsPath.HeroesConfig);
            _heroes = list
                .Where(x => x != null && x.Id != HeroId.None)
                .ToDictionary(x => x.Id, x => x);
        }

        public HeroConfig ForHero(HeroId id) =>
            _heroes != null && _heroes.TryGetValue(id, out var cfg) ? cfg : GetDefaultHero();

        public IReadOnlyList<HeroConfig> AllHeroes() =>
            _heroes?.Values.ToList() ?? new List<HeroConfig>();

        public HeroConfig GetDefaultHero()
        {
            if (_heroes == null || _heroes.Count == 0)
            {
                Debug.LogError("[StaticDataService] Heroes not loaded or empty. Check Resources/StaticData/Heroes");
                return null;
            }

            return _heroes.Values.First();
        }

        public void LoadAll()
        {
            LoadWeapons();
            LoadMonsters();
            LoadUpgrades();
            LoadHeroes();
    

        }
    }
}
