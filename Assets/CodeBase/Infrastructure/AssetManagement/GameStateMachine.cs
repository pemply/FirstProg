using System;
using System.Collections.Generic;

namespace CodeBase.Infrastructure.AssetManagement
{
    public class GameStateMachine
    {
        private Dictionary<Type, IExitableState> _states;
        private IExitableState _activeState;
        private SceneLoader _sceneLoader;

        public  GameStateMachine(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
            _states = new Dictionary<Type, IExitableState>()
            {
                [typeof(BootstrapState)] =  new BootstrapState(this, _sceneLoader),
                [typeof(LoadLevelState)] =  new LoadLevelState(this, _sceneLoader),
                
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