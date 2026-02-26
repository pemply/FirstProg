using CodeBase.GameLogic;
using CodeBase.Infrastructure.AssetManagement;
using CodeBase.Infrastructure.Services.Pool;
using CodeBase.Infrastructure.Services.Progress;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Infrastructure.Factory
{
    public class PickupFactory
    {
        private readonly IXpService _xp;
        private readonly IPoolService _pool;

        private readonly GameObject _xpPrefab; // ✅ справжній prefab-key

        public PickupFactory(IAssets assets, IXpService xp, IPoolService pool)
        {
            _xp = xp;
            _pool = pool;

            _xpPrefab = assets.LoadPrefab(AssetsPath.XpPickupPath); // ✅ не Instantiate

            
        }

        public GameObject CreateXpPickup(Vector3 at, int amount)
        {
            var go = SpawnXp(at, amount);
            return go;
        }

        public void CreateXpShards(Vector3 at, int totalXp, Transform hero)
        {
            if (totalXp <= 0) return;

            // hero тут по факту не потрібен (магніт робить PickupCollector),
            // але лишаємо сигнатуру, якщо десь так викликається

            int shardCount = Mathf.Clamp(totalXp / 5, 3, 10);

            float bigPart = Random.Range(0.25f, 0.45f);
            int bigPool = Mathf.RoundToInt(totalXp * bigPart);
            int smallPool = totalXp - bigPool;

            int bigCount = Random.value < 0.65f ? 1 : 2;
            bigCount = Mathf.Min(bigCount, shardCount - 1);
            int smallCount = shardCount - bigCount;

            SpawnSplit(at, bigPool, bigCount);
            SpawnSplit(at, smallPool, smallCount);
        }

        private void SpawnSplit(Vector3 at, int pool, int parts)
        {
            if (parts <= 0) return;

            if (pool <= 0)
            {
                for (int i = 0; i < parts; i++)
                    SpawnOne(at, 1);
                return;
            }

            int remaining = pool;

            for (int i = 0; i < parts; i++)
            {
                int left = parts - i;

                int value;
                if (left == 1)
                {
                    value = remaining;
                }
                else
                {
                    int maxForThis = remaining - (left - 1);
                    value = Random.Range(1, Mathf.Max(2, maxForThis));
                }

                remaining -= value;
                SpawnOne(at, Mathf.Max(1, value));
            }
        }

        private void SpawnOne(Vector3 at, int amount)
        {
            Vector3 p = at + new Vector3(
                Random.Range(-0.35f, 0.35f),
                0f,
                Random.Range(-0.35f, 0.35f)
            );

            SpawnXp(p, amount);
        }

        private GameObject SpawnXp(Vector3 pos, int amount)
        {
            if (_xpPrefab == null)
            {
                Debug.LogError("[PickupFactory] XP prefab is null");
                return null;
            }

            GameObject go = _pool.Get(_xpPrefab, pos, Quaternion.identity);

            var pickup = go.GetComponent<XpPickup>();
            if (pickup == null)
            {
                Debug.LogError("[PickupFactory] XpPickup component missing on prefab");
                return go;
            }

            pickup.Construct(amount, _xp);

            // ✅ красиво + без накопичення
            float s = 1f + Mathf.Clamp01(amount / 15f) * 0.6f;
            go.transform.localScale = _xpPrefab.transform.localScale * s;

            return go;
        }
    }
}