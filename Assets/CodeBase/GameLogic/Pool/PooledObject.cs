using CodeBase.Infrastructure.Services.Pool;
using UnityEngine;

namespace CodeBase.GameLogic.Pool
{
    public class PooledObject : MonoBehaviour
    {
        public GameObject PrefabKey { get; private set; }
        private IPoolService _pool;

        private IPoolable[] _poolables; //  кеш

        public void Construct(IPoolService pool, GameObject prefabKey)
        {
            _pool = pool;
            PrefabKey = prefabKey;

            // ✅ один раз на інстанс
            if (_poolables == null || _poolables.Length == 0)
                _poolables = GetComponentsInChildren<IPoolable>(true);
        }

        public void CallSpawned()
        {
            if (_poolables == null) return;
            for (int i = 0; i < _poolables.Length; i++)
                _poolables[i].OnSpawned();
        }

        public void CallDespawned()
        {
            if (_poolables == null) return;
            for (int i = 0; i < _poolables.Length; i++)
                _poolables[i].OnDespawned();
        }

        public void Release()
        {
            if (_pool == null || PrefabKey == null)
            {
                Debug.LogWarning(
                    $"[PooledObject] Release fallback DESTROY name={name} poolNull={_pool==null} keyNull={PrefabKey==null}",
                    this);
                Destroy(gameObject);
                return;
            }

            _pool.Release(gameObject);
        }
    }
}