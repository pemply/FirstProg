using System.Collections.Generic;
using CodeBase.GameLogic.Pool;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Pool
{
    public class PoolService : IPoolService
    {
        // --- Verbose logs (CreateNew/Returned/Get) ---
        private const bool VERBOSE = false;

        // --- Stats log ---
        private const bool STATS_ENABLED = true;
        private const float STATS_INTERVAL = 5f; // раз на N секунд
        private const int STATS_TOP = 8;         // скільки топ-префабів показувати

        // Якщо хочеш логати stats тільки для конкретних префабів — додай їх назви сюди.
        // Якщо список пустий -> лог для всіх.
        private static readonly HashSet<string> StatsNameFilter = new HashSet<string>()
        {
            // "XPPIC",
            // "DamageTextPopup",
            // "AoE_Telegraph",
            // "Healer",
        };
        private readonly HashSet<GameObject> _active = new();
        private readonly Dictionary<GameObject, Transform> _folders = new();
        private readonly Dictionary<GameObject, Stack<GameObject>> _pools = new();

        private readonly Transform _root;

        // ---- stats per prefab key ----
        private class Stats
        {
            public int created;
            public int reused;
            public int gets;
            public int releases;
            public int prewarmed;
        }

        private readonly Dictionary<GameObject, Stats> _stats = new();
        private float _nextStatsTime;

        private static void V(string msg, Object ctx = null)
        {
            if (!VERBOSE) return;
            Debug.Log(msg, ctx);
        }

        public PoolService()
        {
            var go = new GameObject("[Pool]");
            Object.DontDestroyOnLoad(go);
            _root = go.transform;

            _nextStatsTime = Time.unscaledTime + STATS_INTERVAL;
        }

        public void Prewarm(GameObject prefab, int count, Transform parent = null)
        {
            if (prefab == null || count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                var inst = CreateNew(prefab, parent);
                ReturnToPool(inst);
            }

            GetStats(prefab).prewarmed += count;
            V($"[POOL] Prewarm prefab={prefab.name} count={count}");
        }

        public GameObject Get(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (prefab == null) return null;

            if (!_pools.TryGetValue(prefab, out var stack))
            {
                stack = new Stack<GameObject>(16);
                _pools[prefab] = stack;
            }

            bool reused = stack.Count > 0;
            GameObject inst = reused ? stack.Pop() : CreateNew(prefab, parent);

            var st = GetStats(prefab);
            st.gets++;
            if (reused) st.reused++;

            V($"[POOL] Get {inst.name} reused={reused} stackAfterPop={stack.Count}");

            var t = inst.transform;
            t.SetParent(parent, worldPositionStays: false);
            t.SetPositionAndRotation(pos, rot);

            inst.SetActive(true);
            _active.Add(inst);
            inst.GetComponent<PooledObject>()?.CallSpawned();

            MaybeLogStats();

            return inst;
        }

        public void Release(GameObject instance)
        {
            if (instance == null) return;
            _active.Remove(instance); 
            var pooled = instance.GetComponent<PooledObject>();
            if (pooled == null || pooled.PrefabKey == null)
            {
                Debug.LogWarning("[POOL] Release fallback DESTROY (no key)", instance);
                Object.Destroy(instance);
                return;
            }

            pooled.CallDespawned();

            GetStats(pooled.PrefabKey).releases++;

            ReturnToPool(instance);

            MaybeLogStats();
        }
        public void DespawnAllActive()
        {
            if (_active.Count == 0)
                return;

            // копія, бо Release змінює _active
            var list = new List<GameObject>(_active);

            for (int i = 0; i < list.Count; i++)
            {
                var go = list[i];
                if (go != null)
                    Release(go);
            }

            _active.Clear();
        }
        public void Clear()
        {
            foreach (var kv in _pools)
            {
                while (kv.Value.Count > 0)
                {
                    var inst = kv.Value.Pop();
                    if (inst != null) Object.Destroy(inst);
                }
            }

            _pools.Clear();
            _folders.Clear();
            _stats.Clear();
        }

        private static int _seq;

        private GameObject CreateNew(GameObject prefab, Transform parent)
        {
            var inst = Object.Instantiate(prefab, parent != null ? parent : _root);

            var pooled = inst.GetComponent<PooledObject>();
            if (pooled == null)
                pooled = inst.AddComponent<PooledObject>();

            pooled.Construct(this, prefab);

            int id = ++_seq;
            inst.name = $"{prefab.name}#{id}";

            GetStats(prefab).created++;

            V($"[POOL] CreateNew {inst.name}");

            inst.SetActive(false);
            return inst;
        }

        private void ReturnToPool(GameObject inst)
        {
            _active.Remove(inst);
            var pooled = inst.GetComponent<PooledObject>();
            var key = pooled.PrefabKey;

            if (!_pools.TryGetValue(key, out var stack))
            {
                stack = new Stack<GameObject>(16);
                _pools[key] = stack;
            }

            inst.transform.SetParent(GetFolder(key), worldPositionStays: false);
            inst.SetActive(false);
            stack.Push(inst);

            V($"[POOL] Returned {inst.name} stackCount={stack.Count}", inst);
        }

        private Transform GetFolder(GameObject prefabKey)
        {
            if (prefabKey == null) return _root;

            if (_folders.TryGetValue(prefabKey, out var folder) && folder != null)
                return folder;

            var go = new GameObject($"[{prefabKey.name}]");
            go.transform.SetParent(_root, worldPositionStays: false);

            folder = go.transform;
            _folders[prefabKey] = folder;
            return folder;
        }

        private Stats GetStats(GameObject key)
        {
            if (!_stats.TryGetValue(key, out var st))
            {
                st = new Stats();
                _stats[key] = st;
            }
            return st;
        }

        private void MaybeLogStats()
        {
            if (!STATS_ENABLED) return;

            // якщо PoolService створився до першого кадру, Time може бути 0 — норм.
            if (Time.unscaledTime < _nextStatsTime) return;
            _nextStatsTime = Time.unscaledTime + STATS_INTERVAL;

            LogTopStats();
        }

        private void LogTopStats()
        {
            // зберемо список ключів
            var keys = ListPoolKeys();
            if (keys.Count == 0) return;

            // відсортуємо по кількості "gets" (найбільш активні)
            keys.Sort((a, b) =>
            {
                int ga = _stats.TryGetValue(a, out var sa) ? sa.gets : 0;
                int gb = _stats.TryGetValue(b, out var sb) ? sb.gets : 0;
                return gb.CompareTo(ga);
            });

            int shown = 0;
            for (int i = 0; i < keys.Count && shown < STATS_TOP; i++)
            {
                var key = keys[i];
                if (key == null) continue;

                if (StatsNameFilter.Count > 0 && !StatsNameFilter.Contains(key.name))
                    continue;

                if (!_stats.TryGetValue(key, out var st))
                    continue;

                int inPool = _pools.TryGetValue(key, out var stack) ? stack.Count : 0;

                // `[POOL STATS] XPPIC created=250 reused=1200 inPool=180`
                Debug.Log($"[POOL STATS] {key.name} created={st.created} reused={st.reused} inPool={inPool} gets={st.gets} releases={st.releases} prewarmed={st.prewarmed}");

                shown++;
            }
        }

        private List<GameObject> ListPoolKeys()
        {
            // ключі беремо з _stats — бо там всі, що коли-небудь використовувались
            var list = new List<GameObject>(_stats.Count);
            foreach (var kv in _stats)
                list.Add(kv.Key);
            return list;
        }
    }
}