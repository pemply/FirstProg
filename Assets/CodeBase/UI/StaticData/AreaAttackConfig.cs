using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "AreaAttackConfig", menuName = "Static Data/Enemy/Area Attack Config")]
    public class AreaAttackConfig : ScriptableObject
    {
        [Header("Damage/Timing")]
        public float Damage = 10f;
        public float Cooldown = 2.5f;

        [Header("AOE")]
        public float AoERadius = 1.8f;
        public GameObject TelegraphPrefab;

        [Header("Grenade")]
        public GameObject GrenadePrefab;
        public float GrenadeFlightTime = 0.45f;
        public float GrenadeArcHeight = 1.5f;

        [Header("Explosion VFX")]
        public GameObject ExplosionPrefab;

        [Header("Sensor")]
        public float SensorRadius = 8f;

        [Header("Spawn offsets")]
        public float SpawnForwardOffset = 0.25f;
        public float SpawnUpOffset = 0.10f;

        [Header("Ground")]
        public LayerMask GroundMask = ~0;

        [Header("Targeting")]
        public float ForwardLead = 1.5f;
        public float SideOffset = 1.0f;
        public float RandomJitter = 0.5f;
        public float MinDistanceFromHeroMult = 0.8f;
        
        [Header("Animation")]
        [Range(0.5f, 5f)]
        public float ThrowSpeed = 1f;
        
        // AreaAttackConfig
        [Header("Explosion VFX Scale")]
        [Range(0.1f, 10f)] public float ExplosionVfxBaseRadius = 1f; 
    }
}