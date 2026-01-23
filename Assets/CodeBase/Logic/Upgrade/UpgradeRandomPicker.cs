using System.Collections.Generic;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Logic.Upgrade
{
    public static class UpgradeRandomPicker
    {
        public static UpgradeOption[] Pick3(
            IReadOnlyList<UpgradeConfig> pool,
            RunContextService run,
            IStaticDataService staticData)
        {
            var result = new UpgradeOption[3];
            if (pool == null || pool.Count == 0)
                return result;

            var taken = new HashSet<int>();
            int safe = 0;

            for (int i = 0; i < 3 && safe < 300; )
            {
                safe++;

                if (taken.Count >= pool.Count)
                    break;

                int index;
                do
                {
                    index = Random.Range(0, pool.Count);
                } while (taken.Contains(index));

                taken.Add(index);

                var cfg = pool[index];
                if (cfg == null)
                    continue;

                // якщо зброї вже максимум — не показуємо “GetSecondaryWeapon”
                if (cfg.Type == UpgradeType.GetSecondaryWeapon && run.Weapons.Count >= run.MaxWeapons)
                    continue;

                var option = new UpgradeOption
                {
                    Config = cfg,
                    WeaponPreviewId = WeaponId.None
                };

                if (cfg.Type == UpgradeType.GetSecondaryWeapon)
                {
                    var id = PickRandomNewWeaponId(run, staticData);
                    if (id == WeaponId.None)
                        continue; // none => skip

                    option.WeaponPreviewId = id;
                }

                result[i] = option;
                i++;
            }

            return result;
        }

        private static WeaponId PickRandomNewWeaponId(
            RunContextService run,
            IStaticDataService staticData)
        {
            var ids = staticData.AllWeaponIds();

            ids.Remove(WeaponId.None);

            // прибрати всі вже взяті зброї
            for (int i = 0; i < run.Weapons.Count; i++)
                ids.Remove(run.Weapons[i].Id);

            if (ids.Count == 0)
                return WeaponId.None;

            return ids[Random.Range(0, ids.Count)];
        }
    }
}
