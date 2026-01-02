using CodeBase.Infrastructure.Factory;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class GameOverState : IState
    {
        private readonly GameStateMachine _stateMachine;


        public GameOverState(GameStateMachine gameStateMachine )
        {
            _stateMachine = gameStateMachine;
       
        }

        public void Exit()
        {
            
        }

        public void Enter()
        {
            Time.timeScale = 0;
            Debug.Log("Entering GameOverState");
        }
    }
}