using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "Static Data/Monster")]
    public class MonsterStaticData : ScriptableObject
    {
        public MonsterTypeId MonsterTypeId;
    
        [Range(1,100)]
        public int Hp = 50;
    
        [Range(1,30)]
        public float Damage = 10;

        public int MaxLoot;
     
    
        [Range(.5f,5)]
        public float EffectiveDistance = .5f;
    
        [Range(.5f,5)]
        public float Cleavage = .5f;

        [Range(0,10)]
        public float MoveSpeed = 3;
    
        public GameObject PrefabReference;
        
        [Range(0,5f)]
        public float AttackCooldown;
        
        [Range(0, 100)]
        public int XpReward = 10;
    }
}