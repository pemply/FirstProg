using CodeBase.CameraLogic;
using CodeBase.Data;
using CodeBase.Enemy;
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

        public LoadLevelState(
            GameStateMachine stateMachine,
            SceneLoader sceneLoader,
            LoadingCurtain curtain,
            IGameFactory gameFactory,
            IPersistentProgressService progressService,
            RunContextService run,
            IStaticDataService staticData)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _gameFactory = gameFactory;
            _progressService = progressService;

            _run = run;
            _staticData = staticData;

            _startConfig = Resources.Load<GameStartConfig>(AssetsPath.GameStartConfig);
        }

        public void Enter(string sceneName)
        {
            _run.Reset();

            var id = _startConfig != null ? _startConfig.DefaultWeapon : WeaponId.None;
            var weapon = _staticData.GetWeapon(id) ?? _staticData.GetDefaultWeapon();
            var stats = weapon != null ? weapon.BaseStats : default;

            _run.Weapons.Add(new RunContextService.RunWeapon { Id = id, Stats = stats });


            _curtain.Show();
            _gameFactory.Cleanup();
            _sceneLoader.Load(sceneName, OnLoaded);
        }


        public void Exit() =>
            _curtain.Hide();

        private void OnLoaded()
        {
            var hs = _progressService.Progress.heroStats;
            hs.CurrentHP = hs.MaxHP;
            GameObject hero = CreateHero();
            GameObject hud = CreateHud();

            InitProgressReaders();
            BindHudToHero(hud, hero);
            CameraFollow(hero);

            _gameFactory.CreatePillarSpawner();

            InitRunWeapons(hero);

            _stateMachine.Enter<GameLoopState, GameLoopPayload>(new GameLoopPayload(hero, hud));
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

            if (heroHealth == null)
                Debug.LogError("HeroHealth not found on Hero!");
            if (actorUI == null)
                Debug.LogError("ActorUI not found on HUD!");

            if (heroHealth != null && actorUI != null)
                actorUI.Construct(heroHealth);
        }

        private void CameraFollow(GameObject hero)
        {
            Transform target = hero.GetComponentInChildren<CharacterController>().transform;
            Camera.main.GetComponent<CameraFollow>().Follow(target.gameObject);
        }

        private void InitSpawners()
        {
            foreach (GameObject spawnerObj in GameObject.FindGameObjectsWithTag("Spawner"))
            {
                var spawner = spawnerObj.GetComponent<EnemySpawner>();
                _gameFactory.Register(spawner);
            }


        }

        private void InitRunWeapons(GameObject hero)
        {
            
            var spawner = hero.GetComponentInChildren<WeaponVisualSpawner>(true);
            if (spawner == null)
            {
                Debug.LogWarning("WeaponVisualSpawner not found!");
                return;
            }

            if (_run.Weapons.Count == 0)
                return;

            var runWeapon = _run.Weapons[0];

            WeaponConfig cfg = _staticData.GetWeapon(runWeapon.Id);
            if (cfg == null)
                return;

            if (cfg.ViewPrefab != null)
                spawner.SpawnPrimary(cfg.ViewPrefab);

            var heroAttack = hero.GetComponentInChildren<WeaponAttackRunner>(true);
            if (heroAttack != null)
            {
               
                heroAttack.ApplyStats(runWeapon.Stats); // потім дали стати (включно ApplyStats для presentation)
            }

            Debug.Log($"Spawn PRIMARY weapon {runWeapon.Id}");
        }
        

    }
}
