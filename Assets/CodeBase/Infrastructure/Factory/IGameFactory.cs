using System;
using System.Collections.Generic;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public interface IGameFactory : IService
    {
        GameObject CreateHero(GameObject at);
        GameObject CreateHud();
        List<ISavedProgressReader> ProgressReaders { get; }
        List<ISavedProgress> ProgressWriters { get; }
        public Transform HeroTransform { get; set; }
        GameObject HeroGameObject { get; set; }

        void Cleanup();
        void Register(ISavedProgressReader progressReader);
        GameObject CreateMonster(MonsterTypeId monsterTypeId, Transform parent);
        GameObject CreateGameOverWindow();
        GameObject CreateXpPickup(Vector3 at, int amount);
        
        void CreateXpShards(Vector3 at, int totalXp);

        GameObject CreatePillarSpawner();
        GameObject PillarSpawnerGameObject { get; set; }

    }
}