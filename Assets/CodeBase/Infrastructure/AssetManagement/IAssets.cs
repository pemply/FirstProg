using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public interface IAssetProvider
    {
        GameObject Instantiate(string pathHero);
        GameObject Instantiate(string pathHero, Vector3  at);
    }
}