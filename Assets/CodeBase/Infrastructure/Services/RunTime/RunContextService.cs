using System.Collections.Generic;
using CodeBase.Logic.Upgrade;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class RunContextService : IService
    {
        public struct RunWeapon
        {
            public WeaponId Id;
            public WeaponStats Stats;
        }

        // Weapons[0] = primary
        public List<RunWeapon> Weapons { get; } = new();

        public int MaxWeapons { get; set; } = 4; 

        // preview для апгрейда “GetWeapon”
        public WeaponId PendingWeaponId { get; set; } = WeaponId.None;

        public Dictionary<UpgradeType, int> UpgradePicks { get; } = new();

        public int Level { get; set; } = 1;
        public int XpInLevel { get; set; } = 0;

        public float ElapsedSeconds { get; private set; }

        
        public int GetPicks(UpgradeType type) =>
            UpgradePicks.TryGetValue(type, out var v) ? v : 0;

        public void AddPick(UpgradeType type) =>
            UpgradePicks[type] = GetPicks(type) + 1;

        public void Reset()
        {
            Weapons.Clear();
            PendingWeaponId = WeaponId.None;

            Level = 1;
            XpInLevel = 0;
            ElapsedSeconds = 0f;

            UpgradePicks.Clear();
        }

        public void Tick(float dt) => ElapsedSeconds += dt;
    }
}