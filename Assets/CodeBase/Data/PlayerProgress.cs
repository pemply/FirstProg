using System;
using CodeBase.StaticData;

namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
        public Stats heroStats;
        public WorldData WorldData;
        public KillData KillData;

        public PlayerProgress(string initialLevel)
        {
            KillData = new KillData();
            WorldData = new WorldData(initialLevel);
            heroStats = new Stats();
        }

      
    }
}