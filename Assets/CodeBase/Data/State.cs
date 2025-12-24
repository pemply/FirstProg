using System;

namespace CodeBase.Data
{
    [Serializable]
    public class State
    {
        public  float CurrentHP =100;
        public  float MaxHP = 100;

        public void ResetHP() => 
            CurrentHP = MaxHP;
    }
}