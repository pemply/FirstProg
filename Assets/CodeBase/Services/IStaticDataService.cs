using System.Collections.Generic;
using CodeBase.Infrastructure.Services;

namespace CodeBase.StaticData
{
    public interface IStaticDataService : IService
    {
        void LoadMonsters();
        void LoadWeapons();
        void LoadUpgrades();

        MonsterStaticData ForMonster(MonsterTypeId typeId);

        WeaponConfig GetDefaultWeapon();
        WeaponConfig GetWeapon(string weaponId);

        IReadOnlyList<UpgradeConfig> AllUpgrades { get; }
    }
}