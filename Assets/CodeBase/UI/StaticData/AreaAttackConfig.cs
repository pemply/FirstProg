using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "AreaAttackConfig", menuName = "Static Data/Enemy/Area Attack Config")]
    public class AreaAttackConfig : ScriptableObject
    {
        [Header("Damage")]
        [Range(1f, 100f)] public float Damage = 10f;

        [Header("Timing")]
        [Range(0.2f, 10f)] public float Cooldown = 2.5f;
        [Range(0.1f, 3f)]  public float Windup = 0.9f;

        [Header("AOE")]
        [Range(0.5f, 10f)] public float AoERadius = 1.8f;
        public GameObject TelegraphPrefab;

        [Header("Sensor")]
        [Range(0.5f, 30f)] public float SensorRadius = 8f;

        // 🔥 NEW — поведінка прицілювання
        [Header("Targeting")]
        [Range(0f, 6f)] public float ForwardLead = 1.5f;   // перед героєм
        [Range(0f, 4f)] public float SideOffset = 1.0f;    // вбік
        [Range(0f, 3f)] public float RandomJitter = 0.5f;  // хаос
        [Range(0f, 2f)] public float MinDistanceFromHeroMult = 0.8f; // не під ногами
    }
}
