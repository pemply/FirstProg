using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.PersistentProgress;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        public List<ISavedProgressReader> ProgressReaders { get; } = new List<ISavedProgressReader>();
        public List<ISavedProgress> ProgressWriters { get; } = new List<ISavedProgress>();
        public Transform HeroTransform { get; set; }
        public event Action HeroCreated;

        private readonly IAssets _assets;

        public GameObject HeroGameObject { get; set; }
    


        public GameFactory(IAssets assets)
        {
            _assets = assets;
        }

        public GameObject CreateHero(GameObject at)
        {
            HeroGameObject = InstantiateRegistered(AssetsPath.HeroPath, at.transform.position);
            var cc = HeroGameObject.GetComponentInChildren<CharacterController>();
            HeroTransform = cc != null ? cc.transform : HeroGameObject.transform; 

            HeroCreated?.Invoke();
            return HeroGameObject;
        }


        public GameObject CreateHud() =>
            InstantiateRegistered(AssetsPath.PathHud);


        public void Cleanup()
        {
            ProgressReaders.Clear();
            ProgressWriters.Clear();
        }

        private GameObject InstantiateRegistered(string prefabPath, Vector3 position)
        {
            GameObject gameObject = _assets.Instantiate(prefabPath, position);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }
        private GameObject InstantiateRegistered(string prefabPath)
        {
            GameObject gameObject = _assets.Instantiate(prefabPath);
            RegisterProgressWatchers(gameObject);
            return gameObject;
        }

        private void Register(ISavedProgressReader progressReader)
        {
            if (progressReader is ISavedProgress progressWriters)
                ProgressWriters.Add(progressWriters);

            ProgressReaders.Add(progressReader);
        }

        private void RegisterProgressWatchers(GameObject gameObject)
        {
            foreach (ISavedProgressReader progressReader in gameObject.GetComponentsInChildren<ISavedProgressReader>())
                Register(progressReader);
        }
    }
}