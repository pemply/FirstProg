using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.StaticData;
using CodeBase.StaticData.CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic
{
    public class HeroPassiveApplier
    {
        private readonly RunContextService _run;
        private readonly IPersistentProgressService _progress;

        private static readonly Dictionary<PassiveId, Action<HeroConfig, RunContextService, IPersistentProgressService>> Map =
            new()
            {
                // ----- HERO STATS (apply to progress) -----

                // 10 => +10% max hp
                { PassiveId.MaxHpBonus, (cfg, run, progress) => run.MaxHpPercent += cfg.PassivePercent },

                // 10 => +10% pickup radius
                { PassiveId.PickupRadiusBonus, (cfg, run, progress) =>
                    {
                        var hs = progress.Progress.heroStats;
                        hs.PickupRadius *= (1f + cfg.PassivePercent / 100f);
                    }
                },

                // ----- GLOBAL MODIFIERS (store in run, applied later) -----

                // 10 => -10% cooldown for ALL weapons
                { PassiveId.CooldownMultiplier, (cfg, run, progress) => run.CooldownPercent += cfg.PassivePercent },

                // 10 => +10% damage for ALL weapons
                { PassiveId.DamageMultiplier, (cfg, run, progress) => run.DamagePercent += cfg.PassivePercent },

                // 10 => +10% move speed
                { PassiveId.MoveSpeedMultiplier, (cfg, run, progress) => run.MoveSpeedPercent += cfg.PassivePercent },

                // 10 => +10% xp gain (реалізуєш у XpService пізніше)
                { PassiveId.XpGainMultiplier, (cfg, run, progress) => run.XpGainPercent += cfg.PassivePercent },

                // 10 => 10% lifesteal (реалізація: коли наносиш dmg — лікуєш)
                { PassiveId.LifestealPercent, (cfg, run, progress) => run.LifestealPercent += cfg.PassivePercent },

                // 10 => +10% crit chance (реалізація: при розрахунку dmg)
                { PassiveId.CritChanceBonus, (cfg, run, progress) => run.CritChancePercent += cfg.PassivePercent },
            };

        public HeroPassiveApplier(RunContextService run, IPersistentProgressService progress)
        {
            _run = run;
            _progress = progress;
        }

        public void Apply(HeroConfig cfg)
        {
            if (cfg == null)
            {
                Debug.Log("[PASSIVES] cfg NULL");
                return;
            }

            Debug.Log($"[PASSIVES] apply {cfg.Id} passive={cfg.Passive} percent={cfg.PassivePercent}");

            if (Map.TryGetValue(cfg.Passive, out var apply))
                apply(cfg, _run, _progress);
            else
                Debug.LogError($"[PASSIVES] no handler for {cfg.Passive} (add it to HeroPassiveApplier.Map)");
        }

        // Викликати після ApplyHeroConfigToProgress() і Apply(cfg)
        public void FinalizeProgressStats()
        {
            var hs = _progress.Progress.heroStats;

            if (_run.MaxHpPercent != 0f)
                hs.MaxHP *= (1f + _run.MaxHpPercent / 100f);

            hs.CurrentHP = hs.MaxHP;
        }
    }
}
