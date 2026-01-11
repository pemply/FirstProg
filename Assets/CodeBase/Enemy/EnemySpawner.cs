using CodeBase.Data;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemySpawner : MonoBehaviour, ISavedProgress
    {
        public MonsterTypeId MonsterTypeId;
        public bool Slain;
        public string Id {get; set;}
        private IGameFactory _factory;
        private EnemyDeath _enemyDeath;

         private void Awake()
        {
            Id = GetComponent<UniqueId>().Id;
            _factory = AllServices.Container.Single<IGameFactory>();
        }
        public void LoadProgress(PlayerProgress progress)
        {
            if (Slain) 
                return;

            if (progress.KillData.ClearedSpawners.Contains(Id))
            {
                Slain = true;
                return;
            }

            Spawn();
        }

        private void Spawn()
        {
          var monster = _factory.CreateMonster(MonsterTypeId, transform);
          _enemyDeath = monster.GetComponent<EnemyDeath>();
          _enemyDeath.DeathEvent += Slay;
          //    if (_enemyDeath != null)
          //       _enemyDeath.DeathEvent += Slay;
        }


        private void Slay()
        {
            if (_enemyDeath != null)
                _enemyDeath.DeathEvent -= Slay;
            Slain = true;
        }


        public void UpdateProgress(PlayerProgress progress)
        {
            if (Slain && !progress.KillData.ClearedSpawners.Contains(Id))
                progress.KillData.ClearedSpawners.Add(Id);
        }

    }
    }
