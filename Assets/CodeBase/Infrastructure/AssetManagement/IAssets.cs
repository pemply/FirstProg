using CodeBase.Infrastructure.Services;
using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public interface IAssets : IService
    {
        GameObject Instantiate(string pathHero);
        GameObject Instantiate(string pathHero, Vector3  at);
    }
}