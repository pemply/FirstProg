using CodeBase.Enemy;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Progress
{
    public interface IKillRewardService : IService

    {
    void Register(EnemyDeath death, MonsterTypeId monsterTypeId);

    /// <summary>Відписатися від конкретного ворога (якщо треба прибрати вручну).</summary>
    void Unregister(EnemyDeath death);

    /// <summary>Відписатися від усіх зареєстрованих ворогів.</summary>
    void Cleanup();
    }
}