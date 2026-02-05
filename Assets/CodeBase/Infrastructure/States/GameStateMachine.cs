using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.Logic;
using CodeBase.StaticData;

namespace CodeBase.Infrastructure.States
{
    public class GameStateMachine
    {
        private Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;
        private SceneLoader _sceneLoader;
        private readonly ICoroutineRunner _runner;
 


        public GameStateMachine(SceneLoader sceneLoader, LoadingCurtain curtain, AllServices services, ICoroutineRunner runner  )
        {
            _sceneLoader = sceneLoader;
            _runner = runner;
            _states = new Dictionary<Type, IExitableState>()
            {
                [typeof(BootstrapState)] =  new BootstrapState(this, _sceneLoader, services),
                    [typeof(LoadProgressState)] =
                    new LoadProgressState(
                        this,
                        services.Single<IPersistentProgressService>(),
                        services.Single<ISavedLoadService>()),
                    [typeof(LoadLevelState)] =
                        new LoadLevelState(
                            this,
                            _sceneLoader,
                            curtain,
                            services.Single<IGameFactory>(),
                            services.Single<IPersistentProgressService>(),
                            services.Single<RunContextService>(),
                            services.Single<IStaticDataService>()),
                [typeof(GameLoopState)] =  new GameLoopState(this,services.Single<IGameFactory>(), _runner,services.Single<IXpService>(), services.Single<RunContextService>(), services.Single<IDifficultyScalingService>() ),
                [typeof(UpgradeState)] =  new UpgradeState(this, services.Single<IUpgradeService>(),services.Single<IStaticDataService>(), services.Single<RunContextService>(), services.Single<IPersistentProgressService>()),
                [typeof(GameOverState)] =  new GameOverState(this, services.Single<IGameFactory>()),
                
            };
        }

        public void Enter<TState>() where TState : class, IState
        {
            IState state = ChangeState<TState>();
            state.Enter();
        }
        
        public void Enter<TState, TPayLoad>(TPayLoad payLoad) where TState : class, IPayLoadedState<TPayLoad>
        {
            TState state = ChangeState<TState>();
            state.Enter(payLoad);
        }

        private TState ChangeState<TState>() where TState : class, IExitableState
        {
            _activeState?.Exit();

            TState state = GetState<TState>();
            _activeState = state;

            return state;
        }

        private TState GetState<TState>() where TState : class, IExitableState
        {
            return _states[typeof(TState)] as TState;
        }
    }
}