using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class PickupFactory
    {
        private readonly IAssets _assets;
        private readonly IXpService _xp;

        public PickupFactory(IAssets assets, IXpService xp)
        {
            _assets = assets;
            _xp = xp;
        }

        public GameObject CreateXpPickup(Vector3 at, int amount)
        {
            GameObject go = _assets.Instantiate(AssetsPath.XpPickupPath, at);

            var pickup = go.GetComponent<XpPickup>();
            if (pickup == null)
                Debug.LogError("[PickupFactory] XpPickup component missing on prefab");

            pickup?.Construct(amount, _xp);
            return go;
        }
    }
}