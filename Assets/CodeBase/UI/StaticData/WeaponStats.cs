using System;
using UnityEngine;

namespace CodeBase.StaticData
{
    [Serializable]
    public struct WeaponStats
    {
        public float BaseCooldown;
        public float AttackSpeedMult;

        public float Damage;

        public float Range;
        public float HitWidth;
        public int Pierce;

        public float Knockback;
        public float KnockbackChance;
        public AttackShape Shape;

    
        public enum AttackShape
        {
            Line,
            Cone,
            Aura
        }

        public float Cooldown => BaseCooldown / Mathf.Max(0.01f, AttackSpeedMult);

        public static WeaponStats Default => new WeaponStats
        {
            BaseCooldown = 1f,
            AttackSpeedMult = 1f,
            Damage = 1f,

            Range = 3f,
            HitWidth = 1f,
            Pierce = 0,

            Knockback = 0f,
            KnockbackChance = 0.05f,
            Shape = AttackShape.Cone,

            
        };
    }
}