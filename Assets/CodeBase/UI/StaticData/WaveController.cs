using System;
using System.Collections;
using CodeBase.Infrastructure;
using UnityEngine;

public class WaveController
{
    public event Action WaveFinished;

    private readonly ICoroutineRunner _runner;
    private readonly IWaveSpawner _spawner;

    private Coroutine _spawnRoutine;
    private Coroutine _durationRoutine;

    private WaveConfig _config;

    public WaveController(ICoroutineRunner runner, IWaveSpawner spawner)
    {
        _runner = runner;
        _spawner = spawner;
    }

    public void StartWave(WaveConfig config)
    {
        if (_runner == null) { Debug.LogError("[WaveController] Runner is NULL"); return; }
        if (_spawner == null) { Debug.LogError("[WaveController] Spawner is NULL"); return; }
        if (config == null) { Debug.LogError("[WaveController] WaveConfig is NULL"); return; }

        StopWave();

        _config = config;
        _spawner.ResetAlive();

        _spawnRoutine = _runner.StartCoroutine(SpawnLoop());
        _durationRoutine = _runner.StartCoroutine(DurationTimer());
    }

    public void StopWave()
    {
        if (_spawnRoutine != null)
            _runner.StopCoroutine(_spawnRoutine);

        if (_durationRoutine != null)
            _runner.StopCoroutine(_durationRoutine);

        _spawnRoutine = null;
        _durationRoutine = null;
        _config = null;
    }

    private IEnumerator SpawnLoop()
    {
        // одразу можна спавнити, або зачекати інтервал
        while (_config != null)
        {
            if (_spawner.AliveCount < _config.MaxAlive)
                _spawner.TrySpawn(_config.RollEnemy());

            yield return new WaitForSeconds(_config.SpawnInterval);
        }
    }

    private IEnumerator DurationTimer()
    {
        yield return new WaitForSeconds(_config.Duration);

        StopWave();
        WaveFinished?.Invoke();
    }
}