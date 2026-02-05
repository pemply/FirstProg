using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic.Upgrade
{
    public static class UpgradeRoller
    {
        public static UpgradeRoll[] Roll3(
            UpgradeOption[] options,
            UpgradeRarityChances chances,
            float luckPercent)
        {
            var rolls = new UpgradeRoll[3];
            if (options == null) return rolls;

            for (int i = 0; i < rolls.Length; i++)
            {
                var opt = i < options.Length ? options[i] : default;
                var cfg = opt.Config;
                if (cfg == null)
                    continue;

                var rarity = cfg.IgnoreRarity
                    ? UpgradeRarity.Common
                    : (chances != null ? chances.Roll(luckPercent) : UpgradeRarity.Common);
                Debug.Log($"[ROLL] {cfg.name} type={cfg.Type} rarity={rarity} usesInt={cfg.UsesInt} " +
                          $"CommonF={cfg.CommonF.Min}-{cfg.CommonF.Max} RareF={cfg.RareF.Min}-{cfg.RareF.Max} " +
                          $"EpicF={cfg.EpicF.Min}-{cfg.EpicF.Max} LegF={cfg.LegendaryF.Min}-{cfg.LegendaryF.Max} " +
                          $"CommonI={cfg.CommonI.Min}-{cfg.CommonI.Max} RareI={cfg.RareI.Min}-{cfg.RareI.Max} " +
                          $"EpicI={cfg.EpicI.Min}-{cfg.EpicI.Max} LegI={cfg.LegendaryI.Min}-{cfg.LegendaryI.Max}");

                var roll = new UpgradeRoll
                {
                    Config = cfg,
                    Rarity = rarity,
                    WeaponPreviewId = opt.WeaponPreviewId
                };

                if (cfg.Type != UpgradeType.GetSecondaryWeapon)
                {
                    if (cfg.UsesInt) roll.IntValue = cfg.RollInt(rarity);
                    else roll.FloatValue = cfg.RollFloat(rarity);
                }
              
                rolls[i] = roll;
                Debug.Log($"[VALUE] int={roll.IntValue} float={roll.FloatValue}");

            }

            return rolls;
        }

    }
}