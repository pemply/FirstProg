using System;
using System.Collections.Generic;
using CodeBase.Enemy;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Progress
{
    public class KillRewardService : IKillRewardService
    {   private readonly IXpService _xp;
        private readonly IStaticDataService _staticData;
        private readonly Dictionary<EnemyDeath, Action> _handlers = new();

        public KillRewardService(IXpService xp, IStaticDataService staticData)
        {
            _xp = xp;
            _staticData = staticData;
        }

        public void Register(EnemyDeath death, MonsterTypeId monsterTypeId)
        {
            if (death == null) return;
            if (_handlers.ContainsKey(death)) return;

            int xpReward = 0;
            var monsterData = _staticData.ForMonster(monsterTypeId);
            if (monsterData != null)
                xpReward = monsterData.XpReward;

            Action handler = null;
            handler = () =>
            {
                death.DeathEvent -= handler;
                _handlers.Remove(death);

                if (xpReward > 0)
                    _xp.AddXp(xpReward);
            };

            _handlers[death] = handler;
            death.DeathEvent += handler;
        }

        public void Unregister(EnemyDeath death)
        {
            if (death == null) return;
            if (!_handlers.TryGetValue(death, out var handler)) return;

            death.DeathEvent -= handler;
            _handlers.Remove(death);
        }

        public void Cleanup()
        {
            foreach (var kv in _handlers)
                if (kv.Key != null)
                    kv.Key.DeathEvent -= kv.Value;

            _handlers.Clear();
        }
        }
    }
