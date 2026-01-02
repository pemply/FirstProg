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
        public string WeaponId;
        public WeaponStats WeaponStats;
    }
}