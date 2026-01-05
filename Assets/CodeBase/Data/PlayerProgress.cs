using System;
namespace CodeBase.Data
{
    [Serializable]
    public class PlayerProgress
    {
      
         public Stats heroStats; 
        public  WorldData WorldData;
        public KillData KillData;
        public RunProgressData RunProgressData;


        public PlayerProgress(string initialLevel )
        {
             RunProgressData = new RunProgressData();
            KillData = new  KillData();
    
           

            WorldData =  new WorldData(initialLevel);
            heroStats =  new Stats();
        }

      
    }
}