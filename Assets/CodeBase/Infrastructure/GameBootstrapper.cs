using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public class GameBootstrapper : MonoBehaviour, ICoroutineRunner
    {
        public LoadingCurtain Curtain;
        private Game _game;
        private void Awake()
        {
            _game = new Game(this, Curtain);
            _game.StateMachine.Enter<BootstrapState>();
            DontDestroyOnLoad(this);
        }

   
    }
}