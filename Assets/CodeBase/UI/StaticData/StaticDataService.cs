using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private Dictionary<MonsterTypeId, MonsterStaticData> _monsters;
        private Dictionary<string, WeaponConfig> _weapons;
        private List<UpgradeConfig> _upgrades;

        public IReadOnlyList<UpgradeConfig> AllUpgrades => _upgrades;

        public void LoadMonsters()
        {
            _monsters = Resources
                .LoadAll<MonsterStaticData>("Enemy/StaticData")
                .ToDictionary(x => x.MonsterTypeId, x => x);
        }

        public void LoadWeapons()
        {
            _weapons = Resources
                .LoadAll<WeaponConfig>("Weapon")
                .ToDictionary(x => x.WeaponId, x => x);
        }

        public void LoadUpgrades()
        {
            _upgrades = Resources
                .LoadAll<UpgradeConfig>("stats")
                .ToList();
        }

        public MonsterStaticData ForMonster(MonsterTypeId typeId) =>
            _monsters.TryGetValue(typeId, out MonsterStaticData staticData)
                ? staticData
                : null;

        public WeaponConfig GetDefaultWeapon()
        {
            if (_weapons == null || _weapons.Count == 0)
            {
                Debug.LogError("[StaticDataService] Weapons not loaded or empty. Check Resources/Weapons");
                return null;
            }
            return _weapons.Values.First();
        }

        public WeaponConfig GetWeapon(string weaponId) =>
            _weapons != null && _weapons.TryGetValue(weaponId, out var w) ? w : null;
    }
}