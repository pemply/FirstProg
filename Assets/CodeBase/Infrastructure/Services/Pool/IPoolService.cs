using UnityEngine;

namespace CodeBase.Infrastructure.Services.Pool
{
    public interface IPoolService : IService
    {
        GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null);
        void Release(GameObject instance);
        void Prewarm(GameObject prefab, int count, Transform parent = null);
        void Clear(); // опціонально, на restart run
    }
}