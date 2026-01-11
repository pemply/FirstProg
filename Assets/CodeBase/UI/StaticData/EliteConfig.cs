using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/EliteConfig")]
    public class EliteConfig : ScriptableObject
    {
        [Range(0f, 1f)] public float Chance = 0.1f;

        [Header("Multipliers")] public float HpMult = 2f;
        public float DmgMult = 1.5f;
        public float XpMult = 2f;

        [Header("Visual (optional)")] public float ScaleMult = 1.2f;

        [Header("Caps (safety)")] public float MaxHpMult = 10f;
        public float MaxDmgMult = 5f;
        public float MaxXpMult = 5f;
    }
}