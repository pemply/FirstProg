using System.Collections.Generic;
using CodeBase.Infrastructure.Services;
using CodeBase.Logic.Upgrade;

namespace CodeBase.StaticData
{
    public interface IStaticDataService : IService
    {
        void LoadMonsters();
        void LoadWeapons();
        void LoadUpgrades();

        MonsterStaticData ForMonster(MonsterTypeId typeId);

        WeaponConfig GetDefaultWeapon();
        WeaponConfig GetWeapon(WeaponId weaponId);

        IReadOnlyList<UpgradeConfig> AllUpgrades { get; }
        public List<WeaponId> AllWeaponIds();
        UpgradeRarityChances RarityChances { get; }

    }
}