using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Pool Static Data", fileName = "PoolStaticData")]
    public class PoolStaticData : ScriptableObject
    {

        [Header("UI")]
        public GameObject DamagePopupPrefab;

        [Header("Pickups")]
        public GameObject XpOrbPrefab;

        [Header("Telegraphs/VFX")]
        public GameObject AoETelegraphPrefab;
       
        public GameObject ProjectileImpactPrefab;
        public GameObject ProjectileMuzzlePrefab;
        
        public  GameObject SwordSlashFxPrefab;
        public GameObject KamikazeExplosionPrefab;
    }
}