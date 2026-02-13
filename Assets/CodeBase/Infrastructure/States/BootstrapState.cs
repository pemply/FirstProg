
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.RunTime;
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
            _services.RegisterSingle<IInputService>(InputService());
            _services.RegisterSingle<IAssets>(new AssetProvider());

            // META прогрес (сейв)
            _services.RegisterSingle<IPersistentProgressService>(new PersistentProgressService());

            // RUN контекст (зброя, weapon stats, xp/level, timer)
            _services.RegisterSingle(new RunContextService());

            // Difficulty (без RunTimerService!)
            RegisterDifficultyScaling();

            _services.RegisterSingle<IPillarActivationService>(new PillarActivationService());

            RegisterStaticData(); // IStaticDataService + Load*

            // XP тепер від RunContext, не від прогресу
            _services.RegisterSingle<IXpService>(
                new XpService(_services.Single<RunContextService>())
            );

            // Factory тепер теж потребує RunContext (щоб аплаїти weapon stats після спавну героя)
            _services.RegisterSingle<IGameFactory>(
                new GameFactory(
                    _services.Single<IAssets>(),
                    _services.Single<IStaticDataService>(),
                    _services.Single<IXpService>(),
                    _services.Single<IDifficultyScalingService>(),
                    _services.Single<RunContextService>(),
                    _services.Single<IPersistentProgressService>()
                )
            );

            _services.RegisterSingle<IKillRewardService>(
                new KillRewardService(
                    _services.Single<IStaticDataService>(),
                    _services.Single<IGameFactory>(),
                    _services.Single<RunContextService>()
                )
            );

            RegisterUpgradeService();

            _services.RegisterSingle<ISavedLoadService>(
                new SavedLoadService(
                    _services.Single<IPersistentProgressService>(),
                    _services.Single<IGameFactory>()
                )
            );

            // ❌ IRunResetService / RunResetService більше не реєструємо (reset робиться в LoadLevelState через _run.Reset())
        }


        private void RegisterDifficultyScaling()
        {
            DifficultyConfig cfg = Resources.Load<DifficultyConfig>(AssetsPath.DifConfigPath);
            IDifficultyScalingService difficulty = new DifficultyScalingService(cfg);

            _services.RegisterSingle<IDifficultyScalingService>(difficulty);
        }



        private void RegisterUpgradeService()
        {
            _services.RegisterSingle<IUpgradeService>(
                new UpgradeService(
                    _services.Single<IPersistentProgressService>(), // meta (hp/pickupRadius)
                    _services.Single<IGameFactory>(),
                    _services.Single<RunContextService>(),
                    _services.Single<IStaticDataService>()
                    // run weapon stats
                )
            );
        }

        private void RegisterStaticData()
        {
            IStaticDataService staticData = new StaticDataService();

            staticData.LoadAll();
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