using UnityEngine;
using UnityEngine.AI;
using CodeBase.Infrastructure.Factory;
using CodeBase.StaticData;
using CodeBase.Enemy;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.UI;

public class WaveSpawner : IWaveSpawner
{
    public int AliveCount => _alive;
    private int _alive;

    private readonly IGameFactory _factory;
    private readonly Transform _player;
    private readonly Transform _enemiesRoot;
    private readonly IKillRewardService _killReward;
    private readonly float _minRadius;
    private readonly float _maxRadius;

    private const int MaxSpawnAttempts = 12;

    public WaveSpawner(IGameFactory factory,
        Transform player,
        IKillRewardService killReward,
        Transform enemiesRoot = null,
        float minRadius = 8f,
        float maxRadius = 14f)
    {
        _factory = factory;
        _player = player;
        _enemiesRoot = enemiesRoot;
        _minRadius = minRadius;
        _maxRadius = maxRadius;
        _killReward = killReward;
    }

    public void ResetAlive() => _alive = 0;

    public bool TrySpawn(MonsterTypeId typeId)
    {
        Vector3 spawnPos = GetSpawnPoint();

        GameObject enemy = _factory.CreateMonster(typeId, _enemiesRoot);
        if (enemy == null)
            return false;
// ✅ реєструємо винагороду тут, де є typeId
        var death = enemy.GetComponent<EnemyDeath>();
        _killReward.Register(death, typeId);
        PlaceEnemy(enemy, spawnPos);

        _alive++;

        HookDeath(enemy);
        return true;
    }

    // =============================
    // Placement
    // =============================

    private void PlaceEnemy(GameObject enemy, Vector3 pos)
    {
        var agent = enemy.GetComponent<NavMeshAgent>();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.Warp(pos);
        }
        else
        {
            enemy.transform.position = pos;
        }
    }

    // =============================
    // Spawn point logic
    // =============================

    private Vector3 GetSpawnPoint()
    {
        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(_minRadius, _maxRadius);

            Vector3 rawPos = _player.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            bool ok = NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 50f, NavMesh.AllAreas);
            
            if (ok)
                return hit.position;
        }

        return _player.position + Vector3.forward * _minRadius;
    }


    // =============================
    // Death tracking
    // =============================

    private void HookDeath(GameObject enemy)
    {
        var death = enemy.GetComponent<EnemyDeath>();
        if (death == null)
        {
            Debug.LogError($"[WaveSpawner] Enemy '{enemy.name}' has no EnemyDeath component. AliveCount will desync.");
            return;
        }

        death.DeathEvent += OnDeath;

        void OnDeath()
        {
           
            death.DeathEvent -= OnDeath;
            _alive = Mathf.Max(0, _alive - 1);
        }
    }
}

public interface IWaveSpawner
{
    int AliveCount { get; }
    void ResetAlive();
    bool TrySpawn(MonsterTypeId typeId);
}
