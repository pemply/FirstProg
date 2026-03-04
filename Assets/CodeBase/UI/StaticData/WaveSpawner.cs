using UnityEngine;
using UnityEngine.AI;
using CodeBase.Infrastructure.Factory;
using CodeBase.StaticData;
using CodeBase.Enemy;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.RunTime;

public class WaveSpawner : IWaveSpawner
{
    public int AliveCount => _alive;
    private int _alive;

    private readonly IGameFactory _factory;
    private readonly RunContextService _run;
    private readonly Transform _enemiesRoot;
    private readonly IKillRewardService _killReward;
    private readonly float _minRadius;
    private readonly float _maxRadius;

    private const int MaxSpawnAttempts = 12;

    public WaveSpawner(
        IGameFactory factory,
        RunContextService run,
        IKillRewardService killReward,
        Transform enemiesRoot = null,
        float minRadius = 8f,
        float maxRadius = 14f)
    {
        _factory = factory;
        _run = run;
        _enemiesRoot = enemiesRoot;
        _minRadius = minRadius;
        _maxRadius = maxRadius;
        _killReward = killReward;
    }

    public void ResetAlive() => _alive = 0;

    public bool TrySpawn(MonsterTypeId typeId)
    {
        Transform player = _run.HeroTransform;
        if (player == null)
            return false;

        Vector3 spawnPos = GetSpawnPoint(player);

        GameObject enemy = _factory.CreateMonster(typeId, _enemiesRoot);
        if (enemy == null)
            return false;

        PlaceEnemy(enemy, spawnPos);

        _alive++; // ✅ ОДИН раз

        // ✅ handle на enemy, не на root
        var handle = enemy.GetComponent<AliveCounterHandle>();
        if (handle == null)
            handle = enemy.AddComponent<AliveCounterHandle>();

        handle.Construct(() => _alive = Mathf.Max(0, _alive - 1));

        // ✅ kill reward тільки на смерть
        var death = enemy.GetComponent<EnemyDeath>();
        if (death != null)
        {
            _killReward.Register(death, typeId);

            // ✅ при смерті теж mark gone (щоб disable не добив вдруге)
            death.DeathEvent += OnDeath;

            void OnDeath()
            {
                death.DeathEvent -= OnDeath;
                handle.MarkGone(); // ✅ один шлях з guard
            }
        }
        else
        {
            Debug.LogWarning($"[WaveSpawner] Enemy '{enemy.name}' has no EnemyDeath.", enemy);
        }

        return true;
    }

    private Vector3 GetSpawnPoint(Transform player)
    {
        for (int i = 0; i < MaxSpawnAttempts; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(_minRadius, _maxRadius);

            Vector3 rawPos = player.position + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            if (NavMesh.SamplePosition(rawPos, out NavMeshHit hit, 50f, NavMesh.AllAreas))
                return hit.position;
        }

        return player.position + Vector3.forward * _minRadius;
    }

    private void PlaceEnemy(GameObject enemy, Vector3 pos)
    {
        var agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null && agent.isOnNavMesh) agent.Warp(pos);
        else enemy.transform.position = pos;
    }
}

public interface IWaveSpawner
{
    int AliveCount { get; }
    void ResetAlive();
    bool TrySpawn(MonsterTypeId typeId);
}