
using CodeBase.Infrastructure.AssetManagement;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class PillarFactory
    {
        private readonly IAssets _assets;
        public PillarFactory(IAssets assets)
        {
            _assets = assets;
        }
        public GameObject CreatePillarSpawner()
        {
            GameObject prefab = _assets.Instantiate(AssetsPath.PillarSpawnerPath);
            return prefab;
        }
    }
}