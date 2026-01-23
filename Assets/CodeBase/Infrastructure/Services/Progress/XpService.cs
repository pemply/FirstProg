// CodeBase/Infrastructure/Services/Progress/XpService.cs
using System;
using CodeBase.Data;
using CodeBase.Infrastructure.Services.RunTime;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Progress
{
    public class XpService : IXpService
    {
        private readonly RunContextService _run;

        private const int MaxLevel = 50;
        private const int BaseRequiredXp = 50;
        private const int StepPerLevel = 50;

        public event Action<XpChangedData> Changed;
        public event Action<int> LevelUp;

        public XpService(RunContextService run)
        {
            _run = run;
        }
        public void Refresh() => Notify();

        public int Level => _run.Level;
        public int CurrentXpInLevel => _run.XpInLevel;

        public int RequiredXp => Mathf.Max(1, BaseRequiredXp + (_run.Level - 1) * StepPerLevel);

        public void AddXp(int amount)
        {
            Debug.Log($"[XP] AddXp amount={amount} before={_run.XpInLevel} level={_run.Level} req={RequiredXp}");

            if (amount <= 0)
                return;

            _run.XpInLevel += amount;

            while (_run.Level < MaxLevel && _run.XpInLevel >= RequiredXp)
            {
                _run.XpInLevel -= RequiredXp;
                _run.Level++;

                LevelUp?.Invoke(_run.Level);
            }

            Notify();
        }

        public void ResetRun()
        {
            _run.Level = 1;
            _run.XpInLevel = 0;
            Notify();
        }

        private void Notify() =>
            Changed?.Invoke(new XpChangedData(_run.XpInLevel, RequiredXp, _run.Level));
    }
}