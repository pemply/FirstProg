using System;
using CodeBase.Enemy;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class PillarActivationService : IPillarActivationService
    {
        public event Action<PillarEncounterSpawner> Changed;

        public PillarEncounterSpawner Current { get; private set; }

        public void SetCurrent(PillarEncounterSpawner pillar)
        {
            if (pillar == null) return;
            if (Current == pillar) return;

            Current = pillar;
            Changed?.Invoke(Current);
        }

        public void Clear(PillarEncounterSpawner pillar)
        {
            if (Current != pillar) return;

            Current = null;
            Changed?.Invoke(null);
        }
    }
    
    public interface IPillarActivationService : IService
    {
        event Action<PillarEncounterSpawner> Changed;
        PillarEncounterSpawner Current { get; }

        void SetCurrent(PillarEncounterSpawner pillar);
        void Clear(PillarEncounterSpawner pillar);
    }

}