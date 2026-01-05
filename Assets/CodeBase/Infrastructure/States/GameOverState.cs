using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.UI;
using CodeBase.UI.over;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.States
{
    public class GameOverState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _factory;
        private readonly IRunResetService _runReset;

        private GameObject _window;

        public GameOverState(GameStateMachine gameStateMachine, IGameFactory factory, IRunResetService runReset)
        {
            _stateMachine = gameStateMachine;
            _factory = factory;
            _runReset = runReset;
        }

        public void Enter()
        {
            Time.timeScale = 0f;
            Debug.Log("Entering GameOverState");

            if (_factory == null)
            {
                Debug.LogError("[GameOverState] Factory is null");
                return;
            }

            _window = _factory.CreateGameOverWindow();
            if (_window == null)
            {
                Debug.LogError("[GameOverState] CreateGameOverWindow returned null. Check prefab path and Resources location.");
                return;
            }

            var view = _window.GetComponent<GameOverWindow>();
            if (view == null)
            {
                Debug.LogError("[GameOverState] GameOverWindow component not found on instantiated prefab.");
                return;
            }

            view.Construct(Restart);
        }


        private void Restart()
        {
            Time.timeScale = 1f;

            _runReset.ResetRunToDefaults();

            if (_window != null)
                Object.Destroy(_window);

            // запускаємо run знову через твій flow
            _stateMachine.Enter<LoadLevelState, string>("Main");
        }

        public void Exit()
        {
            if (_window != null)
                Object.Destroy(_window);
            _window = null;
        }
    }
}