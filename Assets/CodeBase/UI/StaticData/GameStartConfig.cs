using CodeBase.StaticData.CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.StaticData
{
    [CreateAssetMenu(menuName = "StaticData/Game Start Config", fileName = "GameStartConfig")]
    public class GameStartConfig : ScriptableObject
    {
        
        public WeaponId DefaultWeapon = WeaponId.Sword;
        public bool ForceDefaultWeapon; //  для тестів
        public HeroId DefaultHeroId = HeroId.Knight;


    }
}