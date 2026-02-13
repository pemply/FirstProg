using System;
using UnityEngine;

namespace CodeBase.StaticData
{
    [Serializable]
    public struct WeaponStats
    {
        public float BaseCooldown;
        public float AttackSpeed;

        public float Damage;

        public float Range;
        public float HitWidth;
        public int Pierce;
    
        public float Knockback;
        public float KnockbackChance;
        public AttackShape Shape;
        public float CritChance;    
        public float CritMultiplier; 
    
        public enum AttackShape
        {
            Line,
            Cone,
            Aura,
            Aim
        }

        public float Cooldown => BaseCooldown;

        public static WeaponStats Default => new WeaponStats
        {
            BaseCooldown = 1f,
            AttackSpeed = 1f,
            Damage = 1f,
          
            Range = 3f,
            HitWidth = 1f,
            Pierce = 0,

            Knockback = 0f,
            KnockbackChance = 0.05f,
            Shape = AttackShape.Cone,

             CritChance = 0.1f,
             CritMultiplier = 1f
        };
    }
}