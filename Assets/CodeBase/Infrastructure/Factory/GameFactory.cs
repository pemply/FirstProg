using System.Collections.Generic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        public List<ISavedProgressReader> ProgressReaders { get; } = new();
        public List<ISavedProgress> ProgressWriters { get; } = new();

        public Transform HeroTransform { get; set; }
        public GameObject HeroGameObject { get; set; }

        private readonly HeroFactory _heroFactory;
        private readonly UIFactory _uiFactory;
        private readonly PickupFactory _pickupFactory;
        private readonly MonsterFactory _monsterFactory;
        private readonly PillarFactory _pillarFactory;
        public GameFactory(IAssets assets, IStaticDataService staticData, IXpService xp, IDifficultyScalingService difficulty )
        {
            _pillarFactory = new PillarFactory(assets);

            _heroFactory = new HeroFactory(assets);
            _uiFactory = new UIFactory(assets);
            _pickupFactory = new PickupFactory(assets, xp);
            _monsterFactory = new MonsterFactory(staticData, difficulty);
        }

        public GameObject CreateHero(GameObject at)
        {
            HeroGameObject = _heroFactory.CreateHero(at, out var heroT);
            HeroTransform = heroT;

            RegisterProgressWatchers(HeroGameObject);
            return HeroGameObject;
        }

        public GameObject CreateHud()
        {
            GameObject hud = _uiFactory.CreateHud();
            RegisterProgressWatchers(hud);
            return hud;
        }

        public GameObject CreateGameOverWindow() =>
            _uiFactory.CreateGameOverWindow();

        public GameObject CreateXpPickup(Vector3 at, int amount) =>
            _pickupFactory.CreateXpPickup(at, amount);

        public GameObject PillarSpawnerGameObject { get; set; }


        public GameObject CreateMonster(MonsterTypeId monsterTypeId, Transform parent)
        {
            GameObject monster = _monsterFactory.CreateMonster(monsterTypeId, parent, HeroTransform);
            RegisterProgressWatchers(monster);
            return monster;
        }

        public void Cleanup()
        {
            ProgressReaders.Clear();
            ProgressWriters.Clear();
        }

        public void Register(ISavedProgressReader progressReader)
        {
            if (progressReader is ISavedProgress writer)
                ProgressWriters.Add(writer);

            ProgressReaders.Add(progressReader);
        }

        private void RegisterProgressWatchers(GameObject gameObject)
        {
            foreach (ISavedProgressReader reader in gameObject.GetComponentsInChildren<ISavedProgressReader>(true))
                Register(reader);
        }
       
        public GameObject CreatePillarSpawner()
        {
            PillarSpawnerGameObject = _pillarFactory.CreatePillarSpawner();
            RegisterProgressWatchers(PillarSpawnerGameObject); // якщо там є ISavedProgressReader
            return PillarSpawnerGameObject;
        }

       

    }
}
