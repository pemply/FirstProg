using CodeBase.Infrastructure.AssetManagement;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class HeroFactory
    {
        private readonly IAssets _assets;

        public HeroFactory(IAssets assets) => _assets = assets;

        public GameObject CreateHero(GameObject at, out Transform heroTransform)
        {
            GameObject hero = _assets.Instantiate(AssetsPath.HeroPath, at.transform.position);

            var cc = hero.GetComponentInChildren<CharacterController>();
            heroTransform = cc != null ? cc.transform : hero.transform;

            return hero;
        }
    }
}