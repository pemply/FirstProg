
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Services.Input;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class BootstrapState : IState
    {
        private const string Initial = "Initial";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly AllServices _services;

        public BootstrapState(GameStateMachine stateMachine, SceneLoader sceneLoader, AllServices services)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _services = services;
            RegisterServices();
        }

        public BootstrapState(AllServices services)
        {
            _services = services;
        }
        
        public void Enter()
        {
            _sceneLoader.Load(Initial, onLoaded: EnterLoadLevel);
        }

        private void EnterLoadLevel()
        {
            _stateMachine.Enter<LoadProgressState>();
        }

        private void RegisterServices()
        {
            RegisterStaticData(); // IStaticDataService (і LoadMonsters/LoadWeapons)
            _services.RegisterSingle<IInputService>(InputService());
            _services.RegisterSingle<IAssets>(new AssetProvider());

            _services.RegisterSingle<IPersistentProgressService>(new PersistentProgressService());

            _services.RegisterSingle<IXpService>(
                new XpService(_services.Single<IPersistentProgressService>())
            );


            _services.RegisterSingle<IGameFactory>(
                new GameFactory(
                    _services.Single<IAssets>(),
                    _services.Single<IStaticDataService>(),
                    _services.Single<IXpService>()
                )
            );
            _services.RegisterSingle<IKillRewardService>(
                    new KillRewardService(_services.Single<IStaticDataService>(),     _services.Single<IGameFactory>())
                );
                
            RegisterUpgradeService();

            _services.RegisterSingle<ISavedLoadService>(
                new SavedLoadService(_services.Single<IPersistentProgressService>(), _services.Single<IGameFactory>()));

            AllServices.Container.RegisterSingle<IRunResetService>(
                new RunResetService(
                    _services.Single<IPersistentProgressService>(),
                    _services.Single<IStaticDataService>()
                )
            );
        }

        private void RegisterUpgradeService()
        {
            _services.RegisterSingle<IUpgradeService>(
                new UpgradeService(
                    _services.Single<IPersistentProgressService>(),
                    _services.Single<IGameFactory>()
                )
            );
        }

        private void RegisterStaticData()
        {
            IStaticDataService staticData = new StaticDataService();
            staticData.LoadMonsters();
            staticData.LoadWeapons();
            staticData.LoadUpgrades();
            _services.RegisterSingle(staticData);
        }

        public void Exit()
        {
        }

        private static IInputService InputService()
        {
            if (Application.isEditor)
                return new StandaloneInputService();
            else
                return new MobileInputService();
        }
    }
}