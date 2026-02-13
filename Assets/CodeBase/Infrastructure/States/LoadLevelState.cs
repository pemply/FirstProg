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
using CodeBase.StaticData.CodeBase.StaticData;
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
        private readonly HeroPassiveApplier _passives;

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
            _passives = new HeroPassiveApplier(run, progressService);

            _startConfig = Resources.Load<GameStartConfig>(AssetsPath.GameStartConfig);
        }

        public void Enter(string sceneName)
        {
            _run.Reset();
            _run.SelectedHeroId = _startConfig != null ? _startConfig.DefaultHeroId : HeroId.None;

            HeroConfig heroCfg = _staticData.ForHero(_run.SelectedHeroId);

            // стартова зброя ВІД ГЕРОЯ 
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
            GameObject hero = CreateHero();
            ApplyHeroMoveSpeed(hero);
            Debug.Log($"[PASSIVES@OnLoaded] runHash={_run.GetHashCode()} hero={_run.SelectedHeroId} " +
                      $"passive={(heroCfg!=null?heroCfg.Passive.ToString():"NULL")} p={(heroCfg!=null?heroCfg.PassivePercent:0f)} " +
                      $"run cd%={_run.CooldownPercent} dmg%={_run.DamagePercent} ms%={_run.MoveSpeedPercent}");

            GameObject hud = CreateHud();

            InitProgressReaders();
            BindHudToHero(hud, hero);
            CameraFollow(hero);
            Debug.Log($"[SCALE] hero root lossyScale={hero.transform.lossyScale}");

            var pickup = hero.GetComponentInChildren<SphereCollider>(true); // якщо у тебе pickup через SphereCollider
            if (pickup != null)
                Debug.Log($"[PICKUP] collider radius(local)={pickup.radius}, lossyScale={pickup.transform.lossyScale}");

            _gameFactory.CreatePillarSpawner();

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
        private void ApplyHeroConfigToProgress()
        {
            HeroConfig cfg = _staticData.ForHero(_run.SelectedHeroId);
            if (cfg == null) return;

            Stats hs = _progressService.Progress.heroStats;

            hs.MaxHP = cfg.MaxHp;
            hs.PickupRadius = cfg.PickupRadius;
            hs.RegenHpPerSec = cfg.RegenHpPerSec;

            // ---- crit бонуси героя ----
            hs.CritChanceBonusPercent = Mathf.Clamp(cfg.CritChanceBonusPercent, 0f, 100f);
            hs.CritMultBonus = Mathf.Max(0f, cfg.CritMultBonus);

            hs.CurrentHP = hs.MaxHP;
        }



        private void ApplyHeroMoveSpeed(GameObject hero)
        {
            if (hero == null) return;

            var heroMove = hero.GetComponentInChildren<CodeBase.Hero.HeroMove>(true);
            if (heroMove == null)
            {
                Debug.LogWarning("[SPEED] HeroMove not found");
                return;
            }

            var cfg = _staticData.ForHero(_run.SelectedHeroId);
            float baseSpeed = cfg != null ? cfg.MoveSpeed : heroMove.MovementSpeed;

            float speedMult = 1f + _run.MoveSpeedPercent / 100f; // 10 => 1.1
            heroMove.MovementSpeed = baseSpeed * speedMult;

            Debug.Log($"[SPEED] base={baseSpeed} ms%={_run.MoveSpeedPercent} final={heroMove.MovementSpeed}");
        }



    }
}
