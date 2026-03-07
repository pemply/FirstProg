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

        private int _pendingXp;
        private bool _levelUpPending;
        private int _overflowXp;

        public event Action<XpChangedData> Changed;
        public event Action<int> LevelUp;

        public XpService(RunContextService run)
        {
            _run = run;
        }

        public int Level => _run.Level;
        public int CurrentXpInLevel => _run.XpInLevel;
        public bool IsLevelUpPending => _levelUpPending;

        public int RequiredXp => Mathf.Max(1, BaseRequiredXp + (_run.Level - 1) * StepPerLevel);

        public void Refresh() => Notify();

        public void AddXp(int amount)
        {
            if (amount <= 0)
                return;

            if (_levelUpPending)
                return;

            float mult = 1f + (_run.XpGainPercent / 100f);
            if (mult < 0f)
                mult = 0f;

            amount = Mathf.RoundToInt(amount * mult);

            if (amount <= 0)
                return;

            _run.XpInLevel += amount;

            int required = RequiredXp;

            if (_run.Level < MaxLevel && _run.XpInLevel >= required)
            {
                _overflowXp = _run.XpInLevel - required;
                _run.XpInLevel = required;
                _levelUpPending = true;

                Notify();
                LevelUp?.Invoke(_run.Level + 1);
                return;
            }

            Notify();
        }

        public void ConfirmLevelUp()
        {
            if (!_levelUpPending)
                return;

            if (_run.Level < MaxLevel)
                _run.Level++;

            _run.XpInLevel = _overflowXp;
            _overflowXp = 0;
            _levelUpPending = false;

            Notify();
        }

        public void ResetRun()
        {
            _run.Level = 1;
            _run.XpInLevel = 0;
            _pendingXp = 0;
            _overflowXp = 0;
            _levelUpPending = false;
            Notify();
        }

        public void AddXpBuffered(int amount)
        {
            if (amount <= 0)
                return;

            _pendingXp += amount;
        }

        public void FlushBuffered()
        {
            if (_pendingXp <= 0)
                return;

            int v = _pendingXp;
            _pendingXp = 0;

            AddXp(v);
        }

        private void Notify() =>
            Changed?.Invoke(new XpChangedData(_run.XpInLevel, RequiredXp, _run.Level));
    }
}