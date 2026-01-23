using System;
using CodeBase.Enemy;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class PillarActivationService : IPillarActivationService
    {
        public event Action<PillarEncounterSpawner> Changed;

        public PillarEncounterSpawner Current { get; private set; }
        public event Action<PillarEncounterSpawner> RewardRequested;

        public void RequestReward(PillarEncounterSpawner pillar) // ✅ ДОДАТИ
        {
            if (pillar == null) return;
            RewardRequested?.Invoke(pillar);
        }

        public void SetCurrent(PillarEncounterSpawner pillar)
        {
            Debug.Log($"[PILLAR SERVICE] SetCurrent pillar={pillar?.name} id={pillar?.Id}", pillar);
            if (pillar == null) return;
            if (Current == pillar) return;

            Current?.SetPrompt(false);
            Current = pillar;
            Current.SetPrompt(true);

            Changed?.Invoke(Current);
        }

        public void Clear(PillarEncounterSpawner pillar)
        {
            if (Current != pillar) return;

            Current?.SetPrompt(false);
            Current = null;

            Changed?.Invoke(null);
        }
        
        
    }
    
    public interface IPillarActivationService : IService
    {
        event Action<PillarEncounterSpawner> Changed;
        PillarEncounterSpawner Current { get; }
        event Action<PillarEncounterSpawner> RewardRequested;
        void RequestReward(PillarEncounterSpawner pillar);

        void SetCurrent(PillarEncounterSpawner pillar);
        void Clear(PillarEncounterSpawner pillar);
    }

}