using UnityEngine;

namespace CodeBase.Logic.Upgrade
{
    [CreateAssetMenu(menuName = "StaticData/Upgrade Rarity Chances")]
    public class UpgradeRarityChances : ScriptableObject
    {
        [Range(0f, 1f)] public float Common = 0.7f;
        [Range(0f, 1f)] public float Rare = 0.2f;
        [Range(0f, 1f)] public float Epic = 0.09f;
        [Range(0f, 1f)] public float Legendary = 0.01f;

        public UpgradeRarity Roll(float luckPercent)
        {
            float m = 1f + Mathf.Max(0f, luckPercent) / 100f; // 1000 => 11

            float wC = Common;
            float wR = Rare * m;
            float wE = Epic * (m * m);
            float wL = Legendary * (m * m * m);

            float sum = wC + wR + wE + wL;
            if (sum <= 0f) return UpgradeRarity.Common;

            float r = Random.value * sum;

            if ((r -= wC) < 0f) return UpgradeRarity.Common;
            if ((r -= wR) < 0f) return UpgradeRarity.Rare;
            if ((r -= wE) < 0f) return UpgradeRarity.Epic;
            return UpgradeRarity.Legendary;
            
        }

    }
}