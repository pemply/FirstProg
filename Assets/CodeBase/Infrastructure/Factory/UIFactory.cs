using CodeBase.Infrastructure.AssetManagement;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class UIFactory
    {
        private readonly IAssets _assets;

        public UIFactory(IAssets assets) => _assets = assets;

        public GameObject CreateHud() => _assets.Instantiate(AssetsPath.PathHud);

        public GameObject CreateGameOverWindow() => _assets.Instantiate(AssetsPath.GameOverWindowPath);
    }
}