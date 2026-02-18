using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "HealerConfig", menuName = "Static Data/Enemy/Healer Config")]
    public class HealerConfig : ScriptableObject
    {
        [Header("Heal")]
        [Range(0.1f, 10f)] public float Cooldown = 2f;
        [Range(1f, 200f)]  public float HealAmount = 8f;
        [Range(1f, 30f)]   public float HealRadius = 10f;

        [Header("Positioning")]
        [Range(0.5f, 20f)] public float BehindDistance = 4f;     // позаду бійця
        [Range(0.5f, 30f)] public float KeepFromHero = 10f;      // не підбігати до героя ближче

        [Header("FX")]
        public GameObject HealFxPrefab;
        [Range(0.1f, 5f)]  public float FxLifetime = 1f;
        public Vector3 FxOffset = new Vector3(0f, 1.5f, 0f);
    }
}