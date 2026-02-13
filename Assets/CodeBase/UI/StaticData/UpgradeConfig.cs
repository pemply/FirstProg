using System;
using CodeBase.Logic.Upgrade;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Upgrade")]
    public class UpgradeConfig : ScriptableObject
    {
        public bool IgnoreRarity;
        public UpgradeType Type;
        public int MaxPicks = 0; // 0 = без ліміту
        public string TitleOverride;

        [Serializable] public struct RangeF { public float Min, Max; }
        [Serializable] public struct RangeI { public int Min, Max; }

        [Header("Value kind")]
        public bool UsesInt; // true для Hp/Pierce; false для решти float

        [Header("Float ranges")]
        public RangeF CommonF, RareF, EpicF, LegendaryF;

        [Header("Int ranges")]
        public RangeI CommonI, RareI, EpicI, LegendaryI;

        public string GetTitle()
        {
            if (!string.IsNullOrEmpty(TitleOverride))
                return TitleOverride;

            return Type switch
            {
                UpgradeType.Hp => "HP",
                UpgradeType.WeaponDamage => "Damage",
                UpgradeType.WeaponCooldown => "Cooldown",
                UpgradeType.PickupRadius => "Pickup Radius",
                UpgradeType.KnockbackChance => "Knockback Chance",
                UpgradeType.Knockback => "Knockback",
                UpgradeType.WeaponPierce => "Pierce",
                UpgradeType.WeaponRange => "Range",
                UpgradeType.WeaponWidth => "Width",
                UpgradeType.GetSecondaryWeapon => "Get Weapon",
                UpgradeType.Luck => "Luck",
                UpgradeType.RegenHp => "RegenHp",

                _ => Type.ToString()
            };
        }

        public float RollFloat(UpgradeRarity rarity)
        {
            RangeF r = rarity switch
            {
                UpgradeRarity.Common => CommonF,
                UpgradeRarity.Rare => RareF,
                UpgradeRarity.Epic => EpicF,
                UpgradeRarity.Legendary => LegendaryF,
                _ => CommonF
            };
            // якщо Min==Max — ок
            return UnityEngine.Random.Range(r.Min, r.Max);
        }

        public int RollInt(UpgradeRarity rarity)
        {
            RangeI r = rarity switch
            {
                UpgradeRarity.Common => CommonI,
                UpgradeRarity.Rare => RareI,
                UpgradeRarity.Epic => EpicI,
                UpgradeRarity.Legendary => LegendaryI,
                _ => CommonI
            };
            if (r.Max < r.Min) r.Max = r.Min;
            return UnityEngine.Random.Range(r.Min, r.Max + 1);
        }
    }
}
