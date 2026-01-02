using CodeBase.Hero;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "Static Data/Weapons/Weapon Config", fileName = "Weapon_")]
    public class WeaponConfig : ScriptableObject
    {
        public string WeaponId = "default";
        public WeaponStats BaseStats = WeaponStats.Default;
    }
}