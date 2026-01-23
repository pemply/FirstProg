using System.Collections.Generic;
using System.Linq;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Logic.Upgrade;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private Dictionary<MonsterTypeId, MonsterStaticData> _monsters;
        private Dictionary<WeaponId, WeaponConfig> _weapons;
        private List<UpgradeConfig> _upgrades;
        public IReadOnlyList<UpgradeConfig> AllUpgrades => _upgrades;
        private UpgradeRarityChances _rarityChances;
        public UpgradeRarityChances RarityChances => _rarityChances;
        public void LoadMonsters()
        {
            _monsters = Resources
                .LoadAll<MonsterStaticData>("Enemy/StaticData")
                .ToDictionary(x => x.MonsterTypeId, x => x);
        }
        public List<WeaponId> AllWeaponIds() =>
            _weapons != null ? _weapons.Keys.ToList() : new List<WeaponId>();

        public void LoadWeapons()
        {
            _weapons = new Dictionary<WeaponId, WeaponConfig>();

            foreach (var w in Resources.LoadAll<WeaponConfig>("Weapon"))
            {
                if (w == null) continue;
                _weapons[w.WeaponId] = w;
            }
        }

        public void LoadUpgrades()
        {
            _upgrades = Resources
                .LoadAll<UpgradeConfig>("stats")
                .ToList();
            _rarityChances = Resources.Load<UpgradeRarityChances>(AssetsPath.ChanceRarityUpg);            
        }

        public MonsterStaticData ForMonster(MonsterTypeId typeId) =>
            _monsters != null && _monsters.TryGetValue(typeId, out var staticData) ? staticData : null;

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
    }
}