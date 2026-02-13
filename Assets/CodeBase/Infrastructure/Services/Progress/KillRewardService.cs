using System;
using System.Collections.Generic;
using CodeBase.Enemy;
using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Progress
{
    public class KillRewardService : IKillRewardService
    {
        private readonly IStaticDataService _staticData;
        private readonly IGameFactory _factory;
        private readonly RunContextService _run;

        private readonly Dictionary<EnemyDeath, Action> _handlers = new();

        // кеш героя для хіла
        private HeroHealth _heroHealth;

        public KillRewardService(
            IStaticDataService staticData,
            IGameFactory factory,
            RunContextService run)
        {
            _staticData = staticData;
            _factory = factory;
            _run = run;
        }

        public void Register(EnemyDeath death, MonsterTypeId monsterTypeId)
        {
            if (death == null) return;
            if (_handlers.ContainsKey(death)) return;

            Action handler = null;
            handler = () =>
            {
                death.DeathEvent -= handler;
                _handlers.Remove(death);

                int xpReward = 0;

                // 1) XP з інстансу (еліти/скейл)
                var holder = death.GetComponentInParent<XpRewardHolder>();
                if (holder != null)
                    xpReward = holder.Xp;
                else
                {
                    // fallback зі static data
                    var monsterData = _staticData.ForMonster(monsterTypeId);
                    if (monsterData != null)
                        xpReward = monsterData.XpReward;
                }

                // 🔹 XP shard drop
                if (xpReward > 0)
                    _factory.CreateXpShards(death.transform.position, xpReward);

                // 🔹 LIFESTEAL (heal за kill)
                float lifestealPercent = _run.LifestealPercent; // 10 => 10%

                if (lifestealPercent > 0f)
                {
                    // кешуємо hero health
                    if (_heroHealth == null && _factory.HeroGameObject != null)
                        _heroHealth = _factory.HeroGameObject.GetComponentInChildren<HeroHealth>(true);

                    if (_heroHealth != null && !_heroHealth.Equals(null))
                    {
                        float heal = _heroHealth.maxHealth * (lifestealPercent / 100f);

                        if (heal > 0f)
                            _heroHealth.Heal(heal);
                    }
                }
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
