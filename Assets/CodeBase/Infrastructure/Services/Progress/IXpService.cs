
using System;

namespace CodeBase.Infrastructure.Services.Progress
{
    public interface IXpService : IService
    {
        int Level { get; }
        int CurrentXpInLevel { get; }
        int RequiredXp { get; }
        void AddXpBuffered(int amount);
        void FlushBuffered();
        event Action<XpChangedData> Changed;
        event Action<int> LevelUp;

        void AddXp(int amount);
        public void Refresh();
    }
}