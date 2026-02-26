using CodeBase.StaticData.CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Game Start Config", fileName = "GameStartConfig")]
    public class GameStartConfig : ScriptableObject
    {
        public HeroId DefaultHeroId = HeroId.Knight;


    }
}