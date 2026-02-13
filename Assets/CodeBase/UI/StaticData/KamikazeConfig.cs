using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(fileName = "KamikazeConfig", menuName = "Static Data/Enemy/Kamikaze Config")]
    public class KamikazeConfig : ScriptableObject
    {
        [Range(0f, 3f)] public float FuseDelay = 0.6f;
        [Range(0f, 30f)] public float BlinkSpeed = 12f;
        [Range(0f, 0.5f)] public float RadiusPadding = 0.08f;
    }
}