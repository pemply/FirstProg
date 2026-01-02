using System;
using UnityEngine;

namespace CodeBase.StaticData
{[Serializable]
    public struct WeaponStats
    {
        public float BaseCooldown;     // було AttackCooldown
        public float AttackSpeedMult;  // нове (1 = без бонусу)

        public float Damage;
        public float DamageRadius;

        public float Cooldown => BaseCooldown / Mathf.Max(0.01f, AttackSpeedMult);

        public static WeaponStats Default => new WeaponStats
        {
            BaseCooldown = 1f,
            AttackSpeedMult = 1f,
            Damage = 1f,
            DamageRadius = 3f
        };
    }
}