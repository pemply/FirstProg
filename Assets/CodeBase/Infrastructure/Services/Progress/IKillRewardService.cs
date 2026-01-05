using CodeBase.Enemy;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Progress
{
    public interface IKillRewardService : IService

    {
    void Register(EnemyDeath death, MonsterTypeId monsterTypeId);
    
    void Unregister(EnemyDeath death);
    
    void Cleanup();
    }
}