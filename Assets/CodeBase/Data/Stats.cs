using System;

namespace CodeBase.Data
{
    [Serializable]
    public class Stats
    {
        public  float CurrentHP =100;
        public  float MaxHP = 100;
        public float PickupRadius = 2.5f;
        public float Luck = 1f;
        public void ResetHP() => 
            CurrentHP = MaxHP;
    }
}