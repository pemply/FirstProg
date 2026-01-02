
using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.States.BetweenStates;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class GameLoopState 
        : IExitableState, IPayLoadedState<GameLoopPayload>, IPayLoadedState<ResumeWavesPayload>
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


        public GameLoopState(
            GameStateMachine stateMachine,
            IGameFactory gameFactory,
            ICoroutineRunner runnerMono,
            IXpService xp)
        {
            _stateMachine = stateMachine;
            _gameFactory = gameFactory;
            _runnerMono = runnerMono;
            _xp = xp;
            
            _killReward = AllServices.Container.Single<IKillRewardService>();
        }
        public void Enter(ResumeWavesPayload payload)
        {
            _currentWaveIndex = payload.WaveIndex;

            Time.timeScale = 1f;

            _upgradeOpen = false;
            _pendingUpgrades--;

            // якщо ще є апгрейди (бо було 2-3 level-up підряд)
            if (_pendingUpgrades > 0)
            {
                OpenUpgrade();
                return;
            }

            StartNextWave();
        }
        
        public void Enter(GameLoopPayload payload)
        {
            _loopPayload = payload;
            _xp.LevelUp += OnLevelUp;
            Debug.Log("Entering GameLoopState");

            _hero = payload.Hero;
            _hud  = payload.Hud;

            // ---------- HERO ----------
            _heroHealth = _hero.GetComponentInChildren<HeroHealth>();
            if (_heroHealth == null)
            {
                Debug.LogError("[GameLoopState] HeroHealth not found on Hero");
                return;
            }

            _heroHealth.DeathEvent += OnHeroDied;

            // ---------- HUD / LEVEL UI ----------
            var levelUi = _hud.GetComponentInChildren<HeroLevelUI>(true);
            if (levelUi == null)
            {
                Debug.LogError("[GameLoopState] HeroLevelUI not found on HUD prefab");
                return;
            }

            levelUi.Construct(AllServices.Container.Single<IXpService>());

            // ---------- WAVES ----------
            LoadWaveData();
            CreateWaveSystem(_hero);
            StartNextWave();
        }

        public void Exit()
        {
            if (_isUpgradeFlow)
            {
                _isUpgradeFlow = false;
                return; // ✅ нічого не чистимо, ми повернемося
            }

            _xp.LevelUp -= OnLevelUp;

            // HERO
            if (_heroHealth != null)
                _heroHealth.DeathEvent -= OnHeroDied;

            // WAVES
            if (_waveController != null)
            {
                _waveController.WaveFinished -= OnWaveFinished;
                _waveController.StopWave();
                _waveController = null;
            }

            // REWARDS
            _killReward?.Cleanup();

            _hero = null;
            _hud = null;
            _heroHealth = null;
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

            WaveSpawner spawner = new WaveSpawner(_gameFactory, center);

            _waveController = new WaveController(_runnerMono, spawner);
            _waveController.WaveFinished += OnWaveFinished;
        }

        private void StartNextWave()
        {
            if (_waveSequence == null|| _waveController == null)
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

            Time.timeScale = 0f;          // ✅ реально “вилітає вікно” і гра стопається
            _waveController?.StopWave();  // ✅ зупиняємо спавн

            _stateMachine.Enter<UpgradeState, UpgradePayload>(
                    new UpgradePayload(_loopPayload, _currentWaveIndex)
                );
        }
        }
    }

