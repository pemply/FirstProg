using System.Collections;
using CodeBase.Enemy;
using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Infrastructure.States.BetweenStates;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class GameLoopState
        : IPayLoadedState<GameLoopPayload>, IPayLoadedState<ResumeWavesPayload>
    {
        private bool _isUpgradeFlow;
        private bool _upgradeOpen;
        private int _pendingUpgrades;

        private GameObject _hero;
        private GameObject _hud;

        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _gameFactory;
        private readonly ICoroutineRunner _runnerMono;

        private HeroHealth _heroHealth;

        private WaveController _waveController;
        private WaveSequence _waveSequence;
        private int _currentWaveIndex;

        private readonly IXpService _xp;
        private readonly IKillRewardService _killReward;
        private GameLoopPayload _loopPayload;

        private readonly RunContextService _run;
        private Coroutine _timerRoutine;

        private readonly IDifficultyScalingService _difficulty;

        public GameLoopState(
            GameStateMachine stateMachine,
            IGameFactory gameFactory,
            ICoroutineRunner runnerMono,
            IXpService xp,
            RunContextService run,
            IDifficultyScalingService difficulty)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _runnerMono = runnerMono;
            _xp = xp;

            _run = run;
            _difficulty = difficulty;

            _killReward = AllServices.Container.Single<IKillRewardService>();
        }

        public void Enter(ResumeWavesPayload payload)
        {
            _upgradeOpen = false;

            _currentWaveIndex = payload.WaveIndex;
            Time.timeScale = 1f;

            _pendingUpgrades--;

            if (_pendingUpgrades > 0)
            {
                OpenUpgrade();
                return;
            }

            StartNextWave();
        }

        private IEnumerator TickTimer()
        {
            while (true)
            {
                _run.Tick(Time.deltaTime);
                _difficulty.Tick(_run.ElapsedSeconds);

                // ✅ XP apply once per frame
                _xp.FlushBuffered();

                yield return null;
            }
        }



        public void Enter(GameLoopPayload payload)
        {
            RegisteredPillars();
            
            _difficulty.Reset();

            _timerRoutine = _runnerMono.StartCoroutine(TickTimer());

            _loopPayload = payload;
            _xp.LevelUp += OnLevelUp;

            _hero = payload.Hero;
            _hud  = payload.Hud;

            // HERO
            _heroHealth = _hero.GetComponentInChildren<HeroHealth>();
            if (_heroHealth == null)
            {
                Debug.LogError("[GameLoopState] HeroHealth not found on Hero");
                return;
            }

            _heroHealth.DeathEvent += OnHeroDied;

            // HUD
            var levelUi = _hud.GetComponentInChildren<HeroLevelUI>(true);
            if (levelUi == null)
            {
                Debug.LogError("[GameLoopState] HeroLevelUI not found on HUD prefab");
                return;
            }

            levelUi.Construct(_xp);
            _xp.Refresh();

            // WAVES
            LoadWaveData();
            CreateWaveSystem(_hero);
            StartNextWave();
        }

        private void RegisteredPillars()
        {
            var spawnerGo = _gameFactory.PillarSpawnerGameObject;
            var spawner = spawnerGo != null ? spawnerGo.GetComponent<PillarSpawner>() : null;

            if (spawner != null)
            {
                spawner.Construct(_gameFactory.HeroTransform, OnPillarCompleted);
                spawner.Spawn();
            }
        }

        private void OnPillarCompleted(PillarEncounterSpawner pillar)
        {
            _pendingUpgrades++;

            if (_upgradeOpen)
                return;

            OpenUpgrade();
        }

        public void Exit()
        {
            if (_isUpgradeFlow)
            {
                _isUpgradeFlow = false;
                return;
            }

            _xp.LevelUp -= OnLevelUp;

            if (_heroHealth != null)
                _heroHealth.DeathEvent -= OnHeroDied;

            if (_waveController != null)
            {
                _waveController.WaveFinished -= OnWaveFinished;
                _waveController.StopWave();
                _waveController = null;
            }

            _killReward?.Cleanup();

            _hero = null;
            _hud = null;
            _heroHealth = null;

            if (_timerRoutine != null)
                _runnerMono.StopCoroutine(_timerRoutine);
        }

        // ---------------- WAVES ----------------

        private void LoadWaveData()
        {
            _waveSequence = Resources.Load<WaveSequence>("Waves/WaveSequence/Wave");

            if (_waveSequence == null || _waveSequence.Waves.Count == 0)
            {
                Debug.LogError("[GameLoopState] WaveSequence not found or empty");
                return;
            }

            _currentWaveIndex = 0;
        }

        private void CreateWaveSystem(GameObject hero)
        {
            Transform center = _gameFactory.HeroTransform != null
                ? _gameFactory.HeroTransform
                : hero.transform;

            var killReward = AllServices.Container.Single<IKillRewardService>();
            WaveSpawner spawner = new WaveSpawner(_gameFactory, center, killReward);

            _waveController = new WaveController(_runnerMono, spawner);
            _waveController.WaveFinished += OnWaveFinished;
        }

        private void StartNextWave()
        {
            if (_waveSequence == null || _waveController == null)
                return;

            if (_currentWaveIndex >= _waveSequence.Waves.Count)
                _currentWaveIndex = _waveSequence.Waves.Count - 1;

            WaveConfig cfg = _waveSequence.Waves[_currentWaveIndex];
            _waveController.StartWave(cfg);
        }

        private void OnWaveFinished()
        {
            _currentWaveIndex++;
            StartNextWave();
        }

        private void OnHeroDied()
        {
            _stateMachine.Enter<GameOverState>();
        }

        private void OnLevelUp(int level)
        {
            _pendingUpgrades++;

            if (_upgradeOpen)
                return;

            OpenUpgrade();
        }

        private void OpenUpgrade()
        {
            _upgradeOpen = true;
            _isUpgradeFlow = true;

            Time.timeScale = 0f;
            _waveController?.StopWave();

            _stateMachine.Enter<UpgradeState, UpgradePayload>(
                new UpgradePayload(_loopPayload, _currentWaveIndex)
            );
        }
    }
}
