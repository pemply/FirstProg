using System.Collections.Generic;
using CodeBase.Infrastructure.Services;
using CodeBase.Logic.Upgrade;
using CodeBase.StaticData.CodeBase.StaticData;

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
        List<WeaponId> AllWeaponIds();
        UpgradeRarityChances RarityChances { get; }

        HeroConfig ForHero(HeroId id);
        IReadOnlyList<HeroConfig> AllHeroes();

        void LoadAll();
    }
}