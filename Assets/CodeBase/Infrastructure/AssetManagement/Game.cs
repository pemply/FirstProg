using CodeBase.Services.Input;

namespace CodeBase.Infrastructure.AssetManagement
{
    public class Game
    {
        public GameStateMachine StateMachine;
        public static IInputService InputService;

        public Game(ICoroutineRunner coroutineRunner)
        {
            StateMachine = new GameStateMachine(new SceneLoader(coroutineRunner));
        }

      
    }
}