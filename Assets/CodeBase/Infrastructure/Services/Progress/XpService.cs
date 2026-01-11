// CodeBase/Infrastructure/Services/Progress/XpService.cs
using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Progress
{
    public class XpService : IXpService
    {
        private readonly IPersistentProgressService _progress;

        private const int MaxLevel = 50;
        private const int BaseRequiredXp = 50;   // можна винести в config
        private const int StepPerLevel = 50;

        private RunProgressData Data => _progress.Progress.RunProgressData;
        public event Action<XpChangedData> Changed;
        public event Action<int> LevelUp;
        
        public XpService(IPersistentProgressService progress)
        {
            _progress = progress;
        }


        public int Level => Data.Level;
        public int CurrentXpInLevel => Data.XpInLevel;

        // крива: 50, 100, 150, ...
        public int RequiredXp => Mathf.Max(1, BaseRequiredXp + (Data.Level - 1) * StepPerLevel);


        public void AddXp(int amount)
        {
            Debug.Log($"[XP] AddXp amount={amount} before={Data.XpInLevel} level={Data.Level} req={RequiredXp}");
            
            if (amount <= 0)
                return;

            Data.XpInLevel += amount;

            bool leveled = false;

            while (Data.Level < MaxLevel && Data.XpInLevel >= RequiredXp)
            {
                Data.XpInLevel -= RequiredXp;
                Data.Level++;
                leveled = true;

                LevelUp?.Invoke(Data.Level);
            }

            Notify();
        }

        public void ResetRun()
        {
            Data.Level = 1;
            Data.XpInLevel = 0;
            Notify();
        }

        private void Notify() =>
            Changed?.Invoke(new XpChangedData(Data.XpInLevel, RequiredXp, Data.Level));
    }
}