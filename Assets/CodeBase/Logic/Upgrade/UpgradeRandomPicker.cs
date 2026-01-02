using System.Collections.Generic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic.Upgrade
{
    public static class UpgradeRandomPicker
    {
        public static UpgradeConfig[] Pick3(IReadOnlyList<UpgradeConfig> pool)
        {
            if (pool == null || pool.Count == 0)
                return new UpgradeConfig[3];

            // беремо без повторів
            var taken = new HashSet<int>();
            var result = new UpgradeConfig[3];

            int tries = 0;
            for (int i = 0; i < 3; i++)
            {
                if (taken.Count >= pool.Count)
                    break;

                int index;
                do
                {
                    index = Random.Range(0, pool.Count);
                    tries++;
                    if (tries > 100) break;
                }
                while (taken.Contains(index));

                taken.Add(index);
                result[i] = pool[index];
            }

            return result;
        }
    }
}