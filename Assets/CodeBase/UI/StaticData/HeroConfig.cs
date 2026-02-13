using CodeBase.StaticData.CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "Game/Hero")]
    public class HeroConfig : ScriptableObject
    {
        [Header("Crit")]
        public float CritChanceBonusPercent = 0f; // +10 = +10%
        public float CritMultBonus = 0f;    
        // +0.5 = x2 -> x2.5
        public HeroId Id;
      
        public string Title;

        [Header("Start weapon")]
        public WeaponId StartWeapon;

        [Header("Base stats")]
        public float MoveSpeed = 5f;
        public float MaxHp = 100;
        public float PickupRadius = 2f;
        public float DamageMultiplier = 1f;
        public float RegenHpPerSec = 0f;
        public PassiveId Passive;
        public float PassivePercent = 0f;   // 10 = +10%, -10 = -10%



        [Header("Visual")]
        public GameObject Prefab;
        public Sprite Portrait;
    }
    namespace CodeBase.StaticData
    {
        public enum HeroId
        {
            None = 0,
            Aura = 1, 
            Knight = 2, 
            Sniper = 3,
        }
        public enum PassiveId
        {
            None = 0,

            // прості модифікатори
            CooldownMultiplier,   // value = 0.85
            DamageMultiplier,     // value = 1.15
            MoveSpeedMultiplier,  // value = 1.10
            PickupRadiusBonus,    // value = +1.0
            MaxHpBonus,           // value = +30

            // “механіки”
            LifestealPercent,     // value = 0.05 (5%)
            CritChanceBonus,      
            XpGainMultiplier      // value = 1.20
        }

    }

}