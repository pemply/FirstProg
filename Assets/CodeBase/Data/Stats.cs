using System;

namespace CodeBase.Data
{
    [Serializable]
    public class Stats
    {
        public  float CurrentHP =100;
        public  float MaxHP = 100;
        

        public void ResetHP() => 
            CurrentHP = MaxHP;
    }
}