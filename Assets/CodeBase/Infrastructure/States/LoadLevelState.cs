using CodeBase.CameraLogic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Infrastructure
{
    public class LoadLevelState : IPayLoadedState<string>
    {

        private const string InitialPointTag = "InitialPoint";
        private readonly GameStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _curtain;
        private readonly IGameFactory _gameFactory;

        public LoadLevelState(GameStateMachine stateMachine, SceneLoader sceneLoader, LoadingCurtain curtain)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
        }

        public void Exit() => 
            _curtain.Hide();

        public void Enter(string sceneName)
        {
            _curtain.Show();
          _sceneLoader.Load(sceneName, OnLoaded);
          
        }

        private void CameraFollow(GameObject hero)
        {
            Transform target = hero.GetComponentInChildren<CharacterController>().transform;
            Camera.main.GetComponent<CameraFollow>().Follow(target.gameObject);
        }


        private void OnLoaded()
        {
            GameObject hero = _gameFactory.CreateHero(at: GameObject.FindGameObjectWithTag(InitialPointTag));
            _gameFactory.CreateHud();
            
            CameraFollow(hero);
            Debug.Log($"[LoadLevelState] heroRoot={hero.name} pos={hero.transform.position}");

            _stateMachine.Enter<GameLoopState>();
        }
    }
}