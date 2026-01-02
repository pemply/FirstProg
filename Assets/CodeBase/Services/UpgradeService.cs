using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic.Upgrade;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class UpgradeService : IUpgradeService
    {
        private readonly IPersistentProgressService _progress;
        private readonly IGameFactory _factory;

        public UpgradeService(IPersistentProgressService progress, IGameFactory factory)
        {
            _progress = progress;
            _factory = factory;
        }

        public void Apply(UpgradeConfig config)
        {
            var p = _progress.Progress;

            switch (config.Type)
            {
                case UpgradeType.Hp:
                    p.heroStats.MaxHP += config.IntValue;
                    ApplyHeroHealth();
                    break;

                case UpgradeType.WeaponDamage:
                    MutateWeapon(s => { s.Damage += config.FloatValue; return s; });
                    break;

                case UpgradeType.WeaponRadius:
                    MutateWeapon(s => { s.DamageRadius += config.FloatValue; return s; });
                    break;

                case UpgradeType.WeaponCooldown:
                    MutateWeapon(s =>
                    {
                        float pct = config.FloatValue;              // 15 = +15% attack speed
                        float mult = 1f + pct / 100f;               // 1.15
                        s.AttackSpeedMult *= mult;                  // множимо, а не додаємо
                        s.AttackSpeedMult = Mathf.Min(3f, s.AttackSpeedMult); // кап (3x)
                        return s;
                    });
                    break;


            }
        }

        private void MutateWeapon(System.Func<WeaponStats, WeaponStats> mutator)
        {
            var p = _progress.Progress;

            var s = p.RunProgressData.WeaponStats;
            s = mutator(s);
            p.RunProgressData.WeaponStats = s;

            var hero = _factory.HeroGameObject;
            if (hero == null) return;

            var attack = hero.GetComponentInChildren<HeroAttack>();
            attack?.ApplyStats(s);
        }

        private void ApplyHeroHealth()
        {
            var hero = _factory.HeroGameObject;
            if (hero == null) return;

            var hp = hero.GetComponentInChildren<HeroHealth>();
            hp?.ApplyStats(_progress.Progress.heroStats);
        }
    }
}
