using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "Static Data/Weapons/Weapon Config", fileName = "Weapon_")]
    public class WeaponConfig : ScriptableObject
    {
        public WeaponId WeaponId;
        public WeaponStats BaseStats = WeaponStats.Default;

        [Header("Visual")]
        public GameObject ViewPrefab; 
        
        [Header("Projectile (for Line weapons)")]
        public GameObject ProjectilePrefab;
        public float ProjectileSpeed = 12f;
        
        [Header("FX")]
        public GameObject PersistentFxPrefab;   // для Aura (постійно)
        public Vector3 PersistentFxOffset;
        public GameObject AttackFxPrefab;       // одноразово на атаку
        public Vector3 AttackFxOffset;
        public float AttackFxLifetime = 0.6f;
        [Header("Persistent FX Scale")]
        public float PersistentFxBaseRange = 1f; // при якому Range scale = 1

    }
}