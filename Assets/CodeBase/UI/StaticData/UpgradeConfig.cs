using CodeBase.Logic.Upgrade;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Upgrade")]
    public class UpgradeConfig : ScriptableObject
    {
            public UpgradeType Type;
            public float FloatValue;
            public int IntValue;

            // опціонально: красиві назви (можеш залишити пустими)
            public string TitleOverride;

            public string GetTitle()
            {
                if (!string.IsNullOrEmpty(TitleOverride))
                    return TitleOverride;

                return Type switch
                {
                    UpgradeType.Hp => "HP",
                    UpgradeType.WeaponDamage => "Damage",
                    UpgradeType.WeaponRadius => "Radius",
                    UpgradeType.WeaponCooldown => "Cooldown",
                    UpgradeType.PickupRadius => "Pickup Radius",
                    _ => Type.ToString()
                };
            }

            public string GetDescription()
            {
                return Type switch
                {
                    UpgradeType.Hp => $"+{IntValue}",
                    UpgradeType.WeaponDamage => $"+{FloatValue:0.##}",
                    UpgradeType.WeaponRadius => $"+{FloatValue:0.##}",
                    UpgradeType.WeaponCooldown => $"+{FloatValue:0.#}% AS",
                    UpgradeType.PickupRadius => $"+{FloatValue:0.##}",
                    _ => ""
                };
            }

            public string GetButtonText() => $"{GetTitle()} {GetDescription()}";
        }
    }
