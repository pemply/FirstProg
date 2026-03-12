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
        public GameObject PersistentFxPrefab;
        public Vector3 PersistentFxOffset;

        public GameObject AttackFxPrefab;
        public Vector3 AttackFxOffset;

        [Header("Attack FX Scale")]
        public float AttackFxBaseRange = 1f;
        public float AttackFxBaseWidth = 1f;
        public float AttackFxScaleMult = 1f;   
        
        [Header("Projectile FX")]
        public GameObject ProjectileMuzzlePrefab;
        public GameObject ProjectileImpactPrefab;
    }
}