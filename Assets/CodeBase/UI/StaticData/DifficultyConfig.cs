using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/DifficultyConfig", fileName = "DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Tier progression")] [Min(0f)] public float FirstUpgradeAfterSeconds = 10f;
        [Min(1f)] public float UpgradeEverySeconds = 10f;

        [Header("Cap")] [Min(0)] public int MaxTier = 10;

        [Header("Multipliers per tier")] [Min(0f)]
        public float HpStep = 0.10f; // +10% per tier

        [Min(0f)] public float DmgStep = 0.10f; // +10%  per tier
        [Min(0f)] public float XpStep = 0.10f; // +10% per tier

      
        public float BaseHpMult = 1f;
        public float BaseDmgMult = 1f;
        public float BaseXpMult = 1f;
        
        [Header("Caps (max multipliers)")]
        [Min(1f)] public float MaxHpMult  = 5f;
        [Min(1f)] public float MaxDmgMult = 3f;
        [Min(1f)] public float MaxXpMult  = 2f;
    }
}