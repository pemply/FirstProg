using CodeBase.CameraLogic;
using CodeBase.Data;
using CodeBase.Enemy;
using CodeBase.GameLogic;
using CodeBase.Hero;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Infrastructure.States.BetweenStates;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;
using CodeBase.Infrastructure.Services.Pool;
using CodeBase.StaticData.CodeBase.StaticData; // 👈 ДОДАЛИ

namespace CodeBase.Infrastructure.States
{
    public class LoadLevelState : IPayLoadedState<string>
    {
        private const string InitialPointTag = "InitialPoint";

        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _curtain;
        private readonly IGameFactory _gameFactory;
        private readonly IPersistentProgressService _progressService;
        private readonly RunContextService _run;
        private readonly IStaticDataService _staticData;
        private readonly GameStartConfig _startConfig;
        private readonly HeroPassiveApplier _passives;
        private readonly IPoolService _pool; // 👈 ДОДАЛИ
        private readonly PoolPrewarmService _prewarm;
        public LoadLevelState(
            GameStateMachine stateMachine,
            SceneLoader sceneLoader,
            LoadingCurtain curtain,
            IGameFactory gameFactory,
            IPersistentProgressService progressService,
            RunContextService run,
            IStaticDataService staticData,
            IPoolService pool, PoolPrewarmService prewarm) // 👈 ДОДАЛИ
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _gameFactory = gameFactory;
            _progressService = progressService;
            _run = run;
            _staticData = staticData;
            _pool = pool; // 👈
            _prewarm = prewarm;

            _passives = new HeroPassiveApplier(run, progressService);
            _startConfig = Resources.Load<GameStartConfig>(AssetsPath.GameStartConfig);
        }

        public void Enter(string sceneName)
        {
            _pool.DespawnAllActive();
            _run.Reset();
            _run.SelectedHeroId = _startConfig != null ? _startConfig.DefaultHeroId : HeroId.None;

            HeroConfig heroCfg = _staticData.ForHero(_run.SelectedHeroId);

            WeaponId startWeaponId = heroCfg != null ? heroCfg.StartWeapon : WeaponId.None;
            var weapon = _staticData.GetWeapon(startWeaponId) ?? _staticData.GetDefaultWeapon();
            var stats = weapon != null ? weapon.BaseStats : default;

            _run.Weapons.Add(new RunContextService.RunWeapon { Id = weapon.WeaponId, Stats = stats });

            _curtain.Show();
            _gameFactory.Cleanup();
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        public void Exit() =>
            _curtain.Hide();

        private void OnLoaded()
        {
            ApplyHeroConfigToProgress();

            var heroCfg = _staticData.ForHero(_run.SelectedHeroId);

            _passives.Apply(heroCfg);
            _passives.FinalizeProgressStats();

            // 1) HUD перший (бо в ньому DamagePopupSpawner)
            GameObject hud = CreateHud();

            // 2) Біндимо попапи + prewarm (перед CreateHero!)
            BindDamagePopups(hud);
            _prewarm.PrewarmAll();
            // 3) Тепер створюємо героя (WeaponStatsApplier вже отримає damagePopups)
            GameObject hero = CreateHero();
            ApplyHeroMoveSpeed(hero);
            Debug.Log($"[HERO] spawned id={hero.transform.GetInstanceID()} pos={hero.transform.position}");
            InitProgressReaders();
            BindHudToHero(hud, hero);
            CameraFollow(hero);

            _gameFactory.CreatePillarSpawner();

            _stateMachine.Enter<GameLoopState, GameLoopPayload>(new GameLoopPayload(hero, hud));
        }

        // 🔥 МІНІМАЛЬНИЙ БІНД
        private void BindDamagePopups(GameObject hud)
        {
            var spawner = hud.GetComponentInChildren<DamagePopupSpawner>(true);
            if (spawner == null)
            {
                Debug.LogError("DamagePopupSpawner not found in HUD");
                return;
            }

            spawner.Construct(_pool);
   

            // 🔥 ОЦЕ КРИТИЧНО (інакше GameFactory лишається з null)
            if (_gameFactory is GameFactory gf)
                gf.SetDamagePopups(spawner);
            else
                Debug.LogError($"_gameFactory is not GameFactory: {_gameFactory.GetType().Name}");

            Debug.Log("[POPUP] pool connected + prewarm + factory bound");
        }
        private GameObject CreateHero()
        {
            GameObject initialPoint = GameObject.FindGameObjectWithTag(InitialPointTag);
            return _gameFactory.CreateHero(initialPoint);
        }

        private GameObject CreateHud() =>
            _gameFactory.CreateHud();

        private void InitProgressReaders()
        {
            foreach (ISavedProgressReader progressReader in _gameFactory.ProgressReaders)
                progressReader.LoadProgress(_progressService.Progress);
        }

        private void BindHudToHero(GameObject hud, GameObject hero)
        {
            HeroHealth heroHealth = hero.GetComponentInChildren<HeroHealth>();
            ActorUI actorUI = hud.GetComponentInChildren<ActorUI>(true);

            if (heroHealth != null && actorUI != null)
                actorUI.Construct(heroHealth);
        }

        private void CameraFollow(GameObject hero)
        {
            Transform target = hero.GetComponentInChildren<CharacterController>().transform;
            Camera.main.GetComponent<CameraFollow>().Follow(target.gameObject);
        }

        private void ApplyHeroConfigToProgress()
        {
            HeroConfig cfg = _staticData.ForHero(_run.SelectedHeroId);
            if (cfg == null) return;

            Stats hs = _progressService.Progress.heroStats;

            hs.MaxHP = cfg.MaxHp;
            hs.PickupRadius = cfg.PickupRadius;
            hs.RegenHpPerSec = cfg.RegenHpPerSec;
            hs.CritChanceBonusPercent = Mathf.Clamp(cfg.CritChanceBonusPercent, 0f, 100f);
            hs.CritMultBonus = Mathf.Max(0f, cfg.CritMultBonus);
            hs.CurrentHP = hs.MaxHP;
        }

        private void ApplyHeroMoveSpeed(GameObject hero)
        {
            if (hero == null) return;

            var heroMove = hero.GetComponentInChildren<HeroMove>(true);
            if (heroMove == null) return;

            var cfg = _staticData.ForHero(_run.SelectedHeroId);
            float baseSpeed = cfg != null ? cfg.MoveSpeed : heroMove.MovementSpeed;
            float speedMult = 1f + _run.MoveSpeedPercent / 100f;
            heroMove.MovementSpeed = baseSpeed * speedMult;
        }
    }
}