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

        public GameObject PillarSpawnerGameObject { get; set; }

        private readonly HeroFactory _heroFactory;
        private readonly UIFactory _uiFactory;
        private readonly PickupFactory _pickupFactory;
        private readonly MonsterFactory _monsterFactory;
        private readonly PillarFactory _pillarFactory;
        private readonly ProjectileFactory _projectileFactory ;
        private readonly IStaticDataService _staticData;

        private readonly RunContextService _run;

        public GameFactory(
            IAssets assets,
            IStaticDataService staticData,
            IXpService xp,
            IDifficultyScalingService difficulty,
            RunContextService run)
        {
            _staticData = staticData;
            _run = run;

            _projectileFactory = new ProjectileFactory(staticData); // ✅ тут створюємо

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

            ApplyWeaponStatsToHero(HeroGameObject);

            RegisterProgressWatchers(HeroGameObject);
            return HeroGameObject;
        }

        private void ApplyWeaponStatsToHero(GameObject hero)
        {
            var applier = hero.GetComponentInChildren<WeaponStatsApplier>(true);
            if (applier == null)
            {
                Debug.LogWarning("[GameFactory] WeaponStatsApplier not found on Hero prefab");
                return;
            }

            applier.Construct(_run, _projectileFactory, _staticData);

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

        public void CreateXpShards(Vector3 at, int totalXp) =>
            _pickupFactory.CreateXpShards(at, totalXp, HeroTransform);

  

        public GameObject CreateMonster(MonsterTypeId monsterTypeId, Transform parent)
        {
            GameObject monster = _monsterFactory.CreateMonster(monsterTypeId, parent, HeroTransform);
            RegisterProgressWatchers(monster);
            return monster;
        }

        public GameObject CreatePillarSpawner()
        {
            PillarSpawnerGameObject = _pillarFactory.CreatePillarSpawner();
            RegisterProgressWatchers(PillarSpawnerGameObject);
            return PillarSpawnerGameObject;
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
    }
}
