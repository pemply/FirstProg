using System.Collections.Generic;
using CodeBase.Logic.Upgrade;
using CodeBase.StaticData;
using CodeBase.StaticData.CodeBase.StaticData;

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
        public HeroId SelectedHeroId { get; set; } = HeroId.None;
        public float CooldownPercent { get; set; } = 0f;
        public float MaxHpPercent { get; set; } = 0f;
        public float DamagePercent { get; set; } = 0f;
        public float MoveSpeedPercent { get; set; } = 0f;
        public float XpGainPercent { get; set; } = 0f;

        public float LifestealPercent { get; set; } = 0f; // 10 => 10% (реалізуєш пізніше в дамажі)
        public float CritChancePercent { get; set; } = 0f; // 10 => +10% (реалізуєш пізніше)

        public float RegenHpPerSec { get; set; } = 0f;
        public int MaxWeapons { get; set; } = 4;

        // preview для апгрейда “GetWeapon”
        public WeaponId PendingWeaponId { get; set; } = WeaponId.None;

        public Dictionary<UpgradeType, int> UpgradePicks { get; } = new();

        public int Level { get; set; } = 1;
        public int XpInLevel { get; set; } = 0;

        public float ElapsedSeconds { get; private set; }

        public bool HeroPassivesApplied { get; set; }

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
            CooldownPercent = 0f;
            MaxHpPercent = 0f;
            DamagePercent = 0f;
            MoveSpeedPercent = 0f;
            XpGainPercent = 0f;
            LifestealPercent = 0f;
            CritChancePercent = 0f;
            RegenHpPerSec = 0f;
            UpgradePicks.Clear();

            SelectedHeroId = HeroId.None;
        }

        public void Tick(float dt) => ElapsedSeconds += dt;
    }
}