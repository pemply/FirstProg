using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic.Upgrade;
using UnityEngine;

namespace CodeBase.StaticData
{
    public class UpgradeService : IUpgradeService
    {
        private readonly IPersistentProgressService _progress;
        private readonly IGameFactory _factory;
        private readonly RunContextService _run;
        private readonly IStaticDataService _staticData;

        private readonly Dictionary<UpgradeType, Action<UpgradeRoll>> _handlers;
     
        public UpgradeService(
            IPersistentProgressService progress,
            IGameFactory factory,
            RunContextService run,
            IStaticDataService staticData)
        {
            _progress = progress;
            _factory = factory;
            _run = run;
            _staticData = staticData;

            _handlers = new Dictionary<UpgradeType, Action<UpgradeRoll>>
            {
                // hero/meta
                [UpgradeType.Hp] = ApplyHp,
                [UpgradeType.PickupRadius] = ApplyPickupRadius,

                // weapon stats (поки тільки primary index 0)
                [UpgradeType.WeaponDamage] = ApplyWeaponDamage,
                [UpgradeType.WeaponCooldown] = ApplyWeaponCooldown,
                [UpgradeType.WeaponRange] = ApplyWeaponRange,
                [UpgradeType.WeaponWidth] = ApplyWeaponWidth,
                [UpgradeType.WeaponPierce] = ApplyWeaponPierce,
                [UpgradeType.Knockback] = ApplyKnockback,
                [UpgradeType.KnockbackChance] = ApplyKnockbackChance,
                [UpgradeType.Luck] = ApplyLuck,
                [UpgradeType.AttackSpeed] = ApplyWeaponAttackSpeed,

                // get weapon
                [UpgradeType.GetSecondaryWeapon] = ApplyGetWeapon,
            };
        }

        private void ApplyLuck(UpgradeRoll roll)
        {
            var stats = _progress.Progress.heroStats;

            float add = roll.Config.UsesInt ? roll.IntValue : roll.FloatValue;
            stats.Luck += add;

            // кап щоб не ламало баланс
            stats.Luck = Mathf.Clamp(stats.Luck, 0f, 1000f);
        }



        public void Apply(UpgradeRoll roll)
        {
            if (roll.Config == null)
                return;

            if (!_handlers.TryGetValue(roll.Config.Type, out var handler))
            {
                Debug.LogError($"[UpgradeService] No handler for {roll.Config.Type}");
                return;
            }

            handler.Invoke(roll);

            _run.AddPick(roll.Config.Type);

            ApplyWeaponToHero();
            ApplyHeroStatsToHero();
        }

        // -------------------- APPLY TO HERO --------------------

        private void ApplyHeroStatsToHero()
        {
            var hero = _factory.HeroGameObject;
            if (hero == null) return;

            var stats = _progress.Progress.heroStats;
            var appliers = hero.GetComponentsInChildren<IHeroStatsApplier>(true);

            for (int i = 0; i < appliers.Length; i++)
                appliers[i].ApplyHeroStats(stats);
        }

        private void ApplyWeaponToHero()
        {
            var hero = _factory.HeroGameObject;
            if (hero == null) return;

            hero.GetComponentInChildren<WeaponStatsApplier>(true)?.ApplyCurrent();
        }

        // -------------------- WEAPON MUTATION (Weapons[]) --------------------

        private void MutateAllWeapons(Func<WeaponStats, WeaponStats> mutator)
        {
            for (int i = 0; i < _run.Weapons.Count; i++)
            {
                var w = _run.Weapons[i];
                w.Stats = mutator(w.Stats);
                _run.Weapons[i] = w;
            }
        }

        private void ApplyWeaponDamage(UpgradeRoll roll) =>
            MutateAllWeapons(s => { s.Damage += roll.FloatValue; return s; });
        private float RollValue(UpgradeRoll roll)
        {
            // якщо UsesInt — беремо IntValue
            // (у тебе часто int зберігається як "x100", судячи по логам типу 469)
            return roll.Config.UsesInt ? roll.IntValue  : roll.FloatValue;
        }

        private void ApplyWeaponCooldown(UpgradeRoll roll) =>
            MutateAllWeapons(s =>
            {
                float value = RollValue(roll);          // наприклад 10 => -10%
                float mult  = 1f - value / 100f;

                s.BaseCooldown = Mathf.Max(0.02f, s.BaseCooldown * mult); // або 0.01f
                return s;
            });


        private void ApplyWeaponAttackSpeed(UpgradeRoll roll) =>
            MutateAllWeapons(s =>
            {
                float value = RollValue(roll);          // наприклад 10 => +10%
                float mult  = 1f + value / 100f;

                s.AttackSpeed = Mathf.Clamp(s.AttackSpeed * mult, 0.1f, 100f);
                return s;
            });


        private void ApplyWeaponRange(UpgradeRoll roll) =>
            MutateAllWeapons(s => { s.Range += roll.FloatValue; return s; });

        private void ApplyWeaponWidth(UpgradeRoll roll) =>
            MutateAllWeapons(s => { s.HitWidth += roll.FloatValue; return s; });

        private void ApplyWeaponPierce(UpgradeRoll roll) =>
            MutateAllWeapons(s => { s.Pierce += roll.IntValue; return s; });

        private void ApplyKnockback(UpgradeRoll roll) =>
            MutateAllWeapons(s => { s.Knockback += roll.FloatValue; return s; });

        private void ApplyKnockbackChance(UpgradeRoll roll) =>
            MutateAllWeapons(s =>
            {
                float add = roll.FloatValue / 100f;
                s.KnockbackChance = Mathf.Clamp01(s.KnockbackChance + add);
                return s;
            });


        // -------------------- META / HERO --------------------

        
        private void ApplyHp(UpgradeRoll roll)
        {
            var stats = _progress.Progress.heroStats;
            stats.MaxHP += roll.IntValue;
            stats.CurrentHP = Mathf.Min(stats.CurrentHP + roll.IntValue, stats.MaxHP);
        }

        private void ApplyPickupRadius(UpgradeRoll roll)
        {
            _progress.Progress.heroStats.PickupRadius += roll.FloatValue;
        }

        // -------------------- GET WEAPON --------------------

        private void ApplyGetWeapon(UpgradeRoll roll)
        {
            if (_run.Weapons.Count >= _run.MaxWeapons)
                return;

            // weapon id приходить з UpgradeState через PendingWeaponId
            var id = _run.PendingWeaponId;
            _run.PendingWeaponId = WeaponId.None;

            if (id == WeaponId.None)
                return;

            // не додавати дубль
            for (int i = 0; i < _run.Weapons.Count; i++)
                if (_run.Weapons[i].Id == id)
                    return;

            var wcfg = _staticData.GetWeapon(id);
            if (wcfg == null)
                return;

            _run.Weapons.Add(new RunContextService.RunWeapon
            {
                Id = id,
                Stats = wcfg.BaseStats
            });
        }
    }
}
