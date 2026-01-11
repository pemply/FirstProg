using System;
using System.Collections.Generic;
using CodeBase.Data;
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
        private Dictionary<UpgradeType, Action<UpgradeConfig>> _handlers;
        public UpgradeService(IPersistentProgressService progress, IGameFactory factory)
        {
            _progress = progress;
            _factory = factory;

            _handlers = new Dictionary<UpgradeType, Action<UpgradeConfig>>
            {
                [UpgradeType.Hp] = ApplyHp,
                [UpgradeType.WeaponDamage] = ApplyWeaponDamage,
                [UpgradeType.WeaponRadius] = ApplyWeaponRadius,
                [UpgradeType.WeaponCooldown] = ApplyWeaponCooldown,
                [UpgradeType.PickupRadius] = ApplyPickupRadius,
                [UpgradeType.Knockback] = ApplyKnockback,
                [UpgradeType.KnockbackChance] = ApplyKnockbackChance,
            };
        }
        private void ApplyKnockbackChance(UpgradeConfig config)
        {
            MutateWeapon(s =>
            {
                float add = config.FloatValue / 100f;  // +5 => +0.05
                s.KnockbackChance = Mathf.Clamp01(s.KnockbackChance + add);
                return s;
            });
        }


        private void ApplyKnockback(UpgradeConfig config)
        {
            MutateWeapon(s =>
            {
                s.Knockback += config.FloatValue;
                return s;
            });
        }


        public void Apply(UpgradeConfig config)
        {
            if (!_handlers.TryGetValue(config.Type, out var handler))
            {
                Debug.LogError($"[UpgradeService] No handler for {config.Type}");
                return;
            }

            handler.Invoke(config);
            ApplyToHero();
        }

        private void MutateWeapon(System.Func<WeaponStats, WeaponStats> mutator)
        {
            PlayerProgress p = _progress.Progress;

            WeaponStats s = p.RunProgressData.WeaponStats;
            s = mutator(s);
            p.RunProgressData.WeaponStats = s;
        }

        private void ApplyToHero()
        {
            var hero = _factory.HeroGameObject;
            if (hero == null) return;

            var progress = _progress.Progress;

            foreach (IStatsApplier applier in hero.GetComponentsInChildren<IStatsApplier>(true))
                applier.Apply(progress);
        }
        
        private void ApplyHp(UpgradeConfig config)
        {
            var stats = _progress.Progress.heroStats;
            stats.MaxHP += config.IntValue;
            stats.CurrentHP = Mathf.Min(stats.CurrentHP + config.IntValue, stats.MaxHP);
        }

        private void ApplyWeaponDamage(UpgradeConfig config)
        {
            MutateWeapon(s => { s.Damage += config.FloatValue; return s; });
        }

        private void ApplyWeaponRadius(UpgradeConfig config)
        {
            MutateWeapon(s => { s.DamageRadius += config.FloatValue; return s; });
        }

        private void ApplyWeaponCooldown(UpgradeConfig config)
        {
            MutateWeapon(s =>
            {
                float mult = 1f + config.FloatValue / 100f;
                s.AttackSpeedMult = Mathf.Min(100f, s.AttackSpeedMult * mult);
                return s;
            });
        }

        private void ApplyPickupRadius(UpgradeConfig config)
        {
            _progress.Progress.heroStats.PickupRadius += config.FloatValue;
        }
    }
}
