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

            // ⚠️ старий метод: без hero, залишаємо як є (якщо десь викликається)
            pickup?.Construct(amount, _xp);
            return go;
        }
    

        // ✅ НОВЕ: пачка shard-ів (варіант 2)
        public void CreateXpShards(Vector3 at, int totalXp, Transform hero)
        {
            if (totalXp <= 0) return;
            if (hero == null)
            {
                CreateXpPickup(at, totalXp);
                return;
            }
            int shardCount = Mathf.Clamp(totalXp / 5, 3, 10); 
            float bigPart = Random.Range(0.25f, 0.45f);

            int bigPool = Mathf.RoundToInt(totalXp * bigPart);
            int smallPool = totalXp - bigPool;

            int bigCount = Random.value < 0.65f ? 1 : 2;
            bigCount = Mathf.Min(bigCount, shardCount - 1);
            int smallCount = shardCount - bigCount;

            // 1) big shards
            SpawnSplit(at, bigPool, bigCount, hero);

            // 2) small shards
            SpawnSplit(at, smallPool, smallCount, hero);
        }

        private void SpawnSplit(Vector3 at, int pool, int parts, Transform hero)
        {
            if (parts <= 0) return;

            if (pool <= 0)
            {
                // мінімум 1xp кожен, щоб було “багато”
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
                    // лишаємо мінімум по 1 на кожен залишок
                    int maxForThis = remaining - (left - 1);
                    value = Random.Range(1, Mathf.Max(2, maxForThis));
                }

                remaining -= value;
                SpawnOne(at, Mathf.Max(1, value));
            }
        }

        private void SpawnOne(Vector3 at, int amount)
        {
            Vector3 p = at + new Vector3(Random.Range(-0.35f, 0.35f), 0f, Random.Range(-0.35f, 0.35f));

            GameObject go = _assets.Instantiate(AssetsPath.XpPickupPath, p);

            var pickup = go.GetComponent<XpPickup>();
            if (pickup == null)
            {
                Debug.LogError("[PickupFactory] XpPickup component missing on prefab");
                return;
            }

            pickup.Construct(amount, _xp);

            // легенький scale під amount
            float s = 1f + Mathf.Clamp01(amount / 15f) * 0.6f;
            go.transform.localScale *= s;
        }
    }
}
