using CodeBase.CameraLogic;
using CodeBase.Hero;
using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public class LoadLevelState : IPayLoadedState<string>
    {
        private const string HeroPath = "Hero/hero";
        private const string PathHud = "Hud/Hud";
        private readonly GameStateMachine _gameStateMachine;
        private readonly SceneLoader _sceneLoader;

        public LoadLevelState(GameStateMachine gameStateMachine, SceneLoader sceneLoader)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
        }

        public void Enter(string sceneName)
        {
          _sceneLoader.Load(sceneName, OnLoaded);
          
        }
        private void CameraFollow(GameObject hero)
        {
            Transform target = hero.GetComponentInChildren<CharacterController>().transform;
            Camera.main.GetComponent<CameraFollow>().Follow(target.gameObject);
        }


        private void OnLoaded()
        {
            GameObject hero = Instantiate(HeroPath);
            Instantiate(PathHud);
            
            CameraFollow(hero);
            Debug.Log($"[LoadLevelState] heroRoot={hero.name} pos={hero.transform.position}");

        }

        private static GameObject Instantiate(string pathHero)
        {
            var heroPrefab = Resources.Load<GameObject>(pathHero);
            return Object.Instantiate(heroPrefab);
        }

        public void Exit()
        {
           
        }

     
    }
}