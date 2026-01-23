using System;
using CodeBase.Hero;
using CodeBase.StaticData;

namespace CodeBase.Data
{
    [Serializable]
    public class RunProgressData
    {
        public int Level;
        public int XpInLevel;
        [NonSerialized] public WeaponId WeaponId;

        public WeaponStats WeaponStats;
    }
}