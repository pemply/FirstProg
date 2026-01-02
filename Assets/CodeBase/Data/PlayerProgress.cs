using System;
using CodeBase.Hero;
using CodeBase.StaticData;
using UnityEngine;
using UnityEngine.Serialization;

namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public WeaponStats WeaponStats;
         public Stats heroStats; 
        public  WorldData WorldData;
        public KillData KillData;
        public RunProgressData RunProgressData;


        public PlayerProgress(string initialLevel )
        {
             RunProgressData = new RunProgressData();
            KillData = new  KillData();
            WeaponStats = new WeaponStats();
           

            WorldData =  new WorldData(initialLevel);
            heroStats =  new Stats();
        }

      
    }
}