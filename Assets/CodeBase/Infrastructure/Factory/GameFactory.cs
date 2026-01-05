using System.Collections.Generic;
using CodeBase.Enemy;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

namespace CodeBase.Infrastructure.Factory
{
    public class GameFactory : IGameFactory
    {
        public List<ISavedProgressReader> ProgressReaders { get; } = new List<ISavedProgressReader>();
        public List<ISavedProgress> ProgressWriters { get; } = new List<ISavedProgress>();
        public Transform HeroTransform { get; set; }

        private readonly IAssets _assets;
        private readonly IXpService _xp;

        private readonly IStaticDataService _staticData;

        public GameObject HeroGameObject { get; set; }
    


        public GameFactory(IAssets assets, IStaticDataService staticData, IXpService xp)
        {
            _assets = assets;
            _staticData = staticData;
            _xp = xp;
        }

        public GameObject CreateHero(GameObject at)
        {
            HeroGameObject = InstantiateRegistered(AssetsPath.HeroPath, at.transform.position);
            var cc = HeroGameObject.GetComponentInChildren<CharacterController>();
            HeroTransform = cc != null ? cc.transform : HeroGameObject.transform; 
            
            return HeroGameObject;
        }
        public GameObject CreateGameOverWindow()
        {
            return _assets.Instantiate(AssetsPath.GameOverWindowPath);
        }
        public GameObject CreateXpPickup(Vector3 at, int amount)
        {
            GameObject go = _assets.Instantiate(AssetsPath.XpPickupPath, at);

            var pickup = go.GetComponent<XpPickup>();
            if (pickup == null)
                Debug.LogError("[GameFactory] XpPickup component missing on prefab");

            pickup?.Construct(amount, _xp);
            return go;
        }

        public GameObject CreateHud()
        {
            return InstantiateRegistered(AssetsPath.PathHud);
        }

        public GameObject CreateMonster(MonsterTypeId monsterTypeId, Transform parent)
        
        {
            MonsterStaticData monsterData = _staticData.ForMonster(monsterTypeId);
            Vector3 spawnPos = parent != null ? parent.position : Vector3.zero;
            Quaternion spawnRot = parent != null ? parent.rotation : Quaternion.identity;

            GameObject monster = Object.Instantiate(monsterData.PrefabReference, spawnPos, spawnRot, parent);

            IHealth health = monster.GetComponent<IHealth>();
            health.currentHealth = monsterData.Hp;
            health.maxHealth = monsterData.Hp;
            
            monster.GetComponent<ActorUI>().Construct(health);
            monster.GetComponent<AgentMoveToPlayer>().Construct(HeroTransform);
            monster.GetComponent<NavMeshAgent>().speed = monsterData.MoveSpeed;

            // Base / Tank (звичайна атака)
            var baseAttack = monster.GetComponent<EnemyAttack>();
            if (baseAttack != null)
            {
                baseAttack.Construct(HeroTransform);
                baseAttack.Damage = monsterData.Damage;
                baseAttack.AttackColdown = monsterData.AttackCooldown;
                baseAttack.Cleavage = monsterData.Cleavage;
                baseAttack.EffectiveDistance = monsterData.EffectiveDistance;
            }

// Kamikaze (самознищення)
            var kamikazeAttack = monster.GetComponent<KamikazeAttack>();
            if (kamikazeAttack != null)
            {
                kamikazeAttack.Construct(HeroTransform);
                kamikazeAttack.Damage = monsterData.Damage;
                kamikazeAttack.AttackColdown = monsterData.AttackCooldown;
                kamikazeAttack.Cleavage = monsterData.Cleavage;
                kamikazeAttack.EffectiveDistance = monsterData.EffectiveDistance;
            }
            return monster; 
        }


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

        public void Register(ISavedProgressReader progressReader)
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