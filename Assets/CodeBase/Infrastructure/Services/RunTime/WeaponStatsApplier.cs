using System.Linq;
using CodeBase.Infrastructure.Factory;
using CodeBase.StaticData;
using CodeBase.Weapon;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class WeaponStatsApplier : MonoBehaviour
    {
        [SerializeField, HideInInspector] private MonoBehaviour[] _slotAppliersMb; // size 4 в інспекторі
        private IWeaponStatsApplier[] _slots;
        private Hero.WeaponVisualSpawner _weaponVisualSpawner;

        private RunContextService _run;
        private ProjectileFactory _projectiles;
        private IStaticDataService _staticData;

        public void Construct(RunContextService run, ProjectileFactory projectiles)
        {
            Construct(run, projectiles, null);
        }

        public void Construct(RunContextService run, ProjectileFactory projectiles, IStaticDataService staticData)
        {
            Debug.Log($"[APPLIER Construct] run={(run != null)} proj={(projectiles != null)} staticData={(staticData != null)}");

            _run = run;
            _projectiles = projectiles;
            _staticData = staticData;

            _weaponVisualSpawner = GetComponentInChildren<Hero.WeaponVisualSpawner>(true);

            _slots = new IWeaponStatsApplier[_slotAppliersMb.Length];
            for (int i = 0; i < _slotAppliersMb.Length; i++)
                _slots[i] = _slotAppliersMb[i] as IWeaponStatsApplier;

            ApplyCurrent();
        }

        public void ApplyCurrent()
        {
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

                _slots[i]?.ApplyStats(w.Stats);
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
            primary.ApplyStats(w0.Stats);

            // якщо ти хочеш керувати драйвером анімації через індекс — ставимо тут
            // (зазвичай primary і є драйвер)
            primary.SetAsAnimationDriver(animDriverIndex == 0 || animDriverIndex == -1);
        }
    }
}

public interface IWeaponStatsApplier
{
    void ApplyStats(WeaponStats stats);
}
