
using System;

namespace CodeBase.Infrastructure.Services.Progress
{
    public interface IXpService : IService
    {
        event Action<XpChangedData> Changed;
        event Action<int> LevelUp;
        int Level { get; }
        int CurrentXpInLevel { get; }
        int RequiredXp { get; }
        bool IsLevelUpPending { get; }
        void AddXpBuffered(int amount);
        void FlushBuffered();

        void AddXp(int amount);
        void ConfirmLevelUp();
        void ResetRun();
        public void Refresh();
    }
}