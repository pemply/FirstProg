using System.Linq;
using CodeBase.Data;
using CodeBase.Hero;
using CodeBase.Infrastructure.Factory;
using CodeBase.StaticData;
using CodeBase.Weapon;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class WeaponStatsApplier : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] _slotAppliersMb; // size 4 в інспекторі
        private IWeaponStatsApplier[] _slots;
        private WeaponVisualSpawner _weaponVisualSpawner;
        private Stats _heroStats;
        private RunContextService _run;
        private ProjectileFactory _projectiles;
        private IStaticDataService _staticData;
        
        public void Construct(RunContextService run, ProjectileFactory projectiles, CodeBase.Data.Stats heroStats)
        {
            Construct(run, projectiles, null, heroStats);
        }

        public void Construct(RunContextService run, ProjectileFactory projectiles, IStaticDataService staticData, CodeBase.Data.Stats heroStats)
        {
            Debug.Log($"[APPLIER Construct] run={(run != null)} proj={(projectiles != null)} staticData={(staticData != null)} heroStats={(heroStats != null)}");

            _run = run;
            _projectiles = projectiles;
            _staticData = staticData;
            _heroStats = heroStats;

            _weaponVisualSpawner = GetComponentInChildren<Hero.WeaponVisualSpawner>(true);

            _slots = new IWeaponStatsApplier[_slotAppliersMb.Length];
            for (int i = 0; i < _slotAppliersMb.Length; i++)
                _slots[i] = _slotAppliersMb[i] as IWeaponStatsApplier;

            ApplyCurrent();
        }

        public void ApplyCurrent()
        {
            Debug.Log($"[APPLIER ApplyCurrent] runHash={_run.GetHashCode()} cd%={_run.CooldownPercent} dmg%={_run.DamagePercent}");

            if (_run == null) return;

            if (_run.Weapons == null || _run.Weapons.Count == 0)
            {
                Debug.LogError("[WeaponStatsApplier] No weapons in run");
                return;
            }

            int weaponsCount = _run.Weapons.Count;

            int animDriverIndex = -1;
            int visualDriverIndex = -1;

            // 1) find drivers (як у тебе було)
            for (int i = 0; i < weaponsCount && i < _slotAppliersMb.Length; i++)
            {
                var w = _run.Weapons[i];
                var cfg = _staticData != null ? _staticData.GetWeapon(w.Id) : null;
                if (cfg == null) continue;

                bool auraPersistent =
                    cfg.BaseStats.Shape == WeaponStats.AttackShape.Aura &&
                    cfg.PersistentFxPrefab != null;

                if (animDriverIndex == -1 && !auraPersistent)
                    animDriverIndex = i;

                if (visualDriverIndex == -1 && cfg.ViewPrefab != null)
                    visualDriverIndex = i;

                if (animDriverIndex != -1 && visualDriverIndex != -1)
                    break;
            }

            // 2) apply visuals in hands
            if (_weaponVisualSpawner != null && _staticData != null)
            {
                GameObject viewPrefab = null;

                if (visualDriverIndex >= 0 && visualDriverIndex < weaponsCount)
                {
                    var id = _run.Weapons[visualDriverIndex].Id;
                    var cfg = _staticData.GetWeapon(id);
                    viewPrefab = cfg != null ? cfg.ViewPrefab : null;
                }

                _weaponVisualSpawner.SpawnPrimary(viewPrefab);
            }

            // 3) apply per-slot runtime data (як у тебе було)
            for (int i = 0; i < _slots.Length; i++)
            {
                bool active = i < weaponsCount;

                if (_slotAppliersMb[i] != null)
                    _slotAppliersMb[i].enabled = active;

                if (!active || _slotAppliersMb[i] == null)
                    continue;

                var w = _run.Weapons[i];
                var id = w.Id;

                if (_slotAppliersMb[i] is IWeaponIdReceiver idRec)
                    idRec.SetWeaponId(id);

                if (_slotAppliersMb[i] is IProjectileFactoryReceiver projRec)
                    projRec.Construct(_projectiles);

                if (_staticData != null && _slotAppliersMb[i] is IWeaponConfigReceiver cfgRec)
                    cfgRec.SetConfig(_staticData.GetWeapon(id));

                if (_slotAppliersMb[i] is IAttackAnimationDriver animDriver)
                    animDriver.SetAsAnimationDriver(i == animDriverIndex);
                Debug.Log($"APPLY {id} cooldown={w.Stats.Cooldown} baseCd={w.Stats.BaseCooldown} atkSpd={w.Stats.AttackSpeed}");

                var stats = w.Stats;
                ApplyHeroGlobalPassives(ref stats);
                Debug.Log($"[APPLIER] run cd%={_run.CooldownPercent} dmg%={_run.DamagePercent}");

                _slots[i]?.ApplyStats(stats);

            }

            // ✅ 4) HARD GUARANTEE: primary WeaponAttackRunner завжди отримує ін’єкції
            ApplyToPrimaryRunner(animDriverIndex);

            Debug.Log(
                $"[APPLIER] weapons={weaponsCount}/{_slots.Length} animDriver={animDriverIndex} visualDriver={visualDriverIndex} " +
                $"{string.Join(", ", _run.Weapons.Select(w => $"{w.Id}/{w.Stats.Shape}"))}");
        }

        private void ApplyToPrimaryRunner(int animDriverIndex)
        {
            // беремо саме реальний WeaponPrimary_Attack (по прапорцю)
            var primary = transform.root
                .GetComponentsInChildren<CodeBase.Hero.WeaponAttackRunner>(true)
                .FirstOrDefault(r => r.IsPrimarySlot);

            if (primary == null)
            {
                Debug.LogError("[WeaponStatsApplier] Primary WeaponAttackRunner not found under hero root");
                return;
            }

            // у твоїй моделі slot0 = primary weapon у run.Weapons
            var w0 = _run.Weapons[0];
            var id0 = w0.Id;

            // ін’єкції, яких тобі бракує
            primary.SetWeaponId(id0);
            primary.Construct(_projectiles);

            if (_staticData != null)
                primary.SetConfig(_staticData.GetWeapon(id0));

            // primary повинен мати статси 0-го слота
            var stats0 = w0.Stats;
            ApplyHeroGlobalPassives(ref stats0);
            primary.ApplyStats(stats0);


            // якщо ти хочеш керувати драйвером анімації через індекс — ставимо тут
            // (зазвичай primary і є драйвер)
            primary.SetAsAnimationDriver(animDriverIndex == 0 || animDriverIndex == -1);
        }
        private void ApplyHeroGlobalPassives(ref WeaponStats stats)
        {
            if (_run == null) return;

            if (_run.CooldownPercent != 0f)
            {
                float speedMult = 1f + _run.CooldownPercent / 100f;   // 100% => 2.0
                stats.BaseCooldown = Mathf.Max(0.02f, stats.BaseCooldown / speedMult);
            }

            if (_run.DamagePercent != 0f)
            {
                float mult = 1f + _run.DamagePercent / 100f;
                stats.Damage *= mult;
            }

            // ---------- HERO CRIT BONUS ----------
            var hs = _heroStats;
            if (hs != null)
            {
                stats.CritChance = Mathf.Clamp(stats.CritChance + hs.CritChanceBonusPercent, 0f, 100f);
                stats.CritMultiplier = Mathf.Max(1f, stats.CritMultiplier + hs.CritMultBonus);
            }

        }



        

    }
}

public interface IWeaponStatsApplier
{
    void ApplyStats(WeaponStats stats);
}
