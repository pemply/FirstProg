using System.Collections.Generic;
using UnityEngine;

namespace CodeBase.Infrastructure.AssetManagement
{
    public class AssetProvider : IAssets
    {
        // кеш щоб не грузити Resources кожен раз
        private readonly Dictionary<string, GameObject> _cache = new();

        public GameObject LoadPrefab(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("[Assets] LoadPrefab path NULL");
                return null;
            }

            // якщо вже грузили — повертаємо
            if (_cache.TryGetValue(path, out var prefab) && prefab != null)
                return prefab;

            prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
                Debug.LogError($"[Assets] Prefab not found at Resources path: {path}");
                return null;
            }

            _cache[path] = prefab;
            return prefab;
        }

        public GameObject Instantiate(string path)
        {
            var prefab = LoadPrefab(path);
            return prefab != null ? Object.Instantiate(prefab) : null;
        }

        public GameObject Instantiate(string path, Vector3 at)
        {
            var prefab = LoadPrefab(path);
            return prefab != null ? Object.Instantiate(prefab, at, Quaternion.identity) : null;
        }
    }
}