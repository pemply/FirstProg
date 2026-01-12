using CodeBase.CameraLogic;
using CodeBase.Enemy;
using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.States.BetweenStates;
using CodeBase.Logic;
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
        public LoadLevelState(
            GameStateMachine stateMachine,
            SceneLoader sceneLoader,
            LoadingCurtain curtain,
            IGameFactory gameFactory,
            IPersistentProgressService progressService)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _gameFactory = gameFactory;
            _progressService = progressService;
        }

        public void Exit() =>
            _curtain.Hide();

        public void Enter(string sceneName)
        {
            _curtain.Show();
            _gameFactory.Cleanup();
            _sceneLoader.Load(sceneName, OnLoaded);
        }

        private void OnLoaded()
        {
            GameObject hero = CreateHero();
            GameObject hud = CreateHud();

            InitProgressReaders();
            BindHudToHero(hud, hero);
            CameraFollow(hero);

            _gameFactory.CreatePillarSpawner();

            AllServices.Container.Single<IXpService>().ResetRun();
            _stateMachine.Enter<GameLoopState, GameLoopPayload>(new GameLoopPayload(hero, hud));
        }

        private Transform HeroPivot(GameObject hero) =>
            hero.GetComponentInChildren<CharacterController>(true).transform;

      


        private void InitSpawners()
        {
            foreach (GameObject spawnerObj in GameObject.FindGameObjectsWithTag("Spawner"))
            {
                var spawner = spawnerObj.GetComponent<EnemySpawner>();
                _gameFactory.Register(spawner);
            }
            
        }

        private GameObject CreateHero()
        {
            GameObject initialPoint = GameObject.FindGameObjectWithTag(InitialPointTag);
            return _gameFactory.CreateHero(at: initialPoint);
        }

        private GameObject CreateHud() =>
            _gameFactory.CreateHud();

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

        private void InitProgressReaders()
        {
            foreach (ISavedProgressReader progressReader in _gameFactory.ProgressReaders)
                progressReader.LoadProgress(_progressService.Progress);
        }

        private void CameraFollow(GameObject hero)
        {
            Transform target = hero.GetComponentInChildren<CharacterController>().transform;
            Camera.main.GetComponent<CameraFollow>().Follow(target.gameObject);
        }
        
        
        
    }
}
