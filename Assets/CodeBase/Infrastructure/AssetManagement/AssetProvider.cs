using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public class AssetProvider : IAssets
    {
        public GameObject Instantiate(string pathHero)
        {
            var heroPrefab = Resources.Load<GameObject>(pathHero);
            return Object.Instantiate(heroPrefab);
        }

        public GameObject Instantiate(string pathHero, Vector3  at)
        {
            var heroPrefab = Resources.Load<GameObject>(pathHero);
            return Object.Instantiate(heroPrefab, at, Quaternion.identity);
        }
    }
}