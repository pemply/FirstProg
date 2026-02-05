using CodeBase.Infrastructure.Factory;
using CodeBase.UI.over;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.States
{
    public class GameOverState : IState
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IGameFactory _factory;

        private GameObject _window;

        public GameOverState(GameStateMachine gameStateMachine, IGameFactory factory)
        {
            _stateMachine = gameStateMachine;
            _factory = factory;
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
                Debug.LogError("[GameOverState] CreateGameOverWindow returned null.");
                return;
            }

            var view = _window.GetComponent<GameOverWindow>();
            if (view == null)
            {
                Debug.LogError("[GameOverState] GameOverWindow component not found on prefab.");
                return;
            }

            view.Construct(Restart);
        }

        private void Restart()
        {
            Time.timeScale = 1f;

            if (_window != null)
                Object.Destroy(_window);

            // Новий ран стартує через LoadLevelState (він сам зробить _run.Reset і дефолти)
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