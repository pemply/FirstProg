using System.Linq;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class WeaponStatsApplier : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] _slotAppliersMb; // size 4 в інспекторі
        private IWeaponStatsApplier[] _slots;
        private RunContextService _run;

        public void Construct(RunContextService run)
        {
            _run = run;
            _slots = new IWeaponStatsApplier[_slotAppliersMb.Length];
            for (int i = 0; i < _slotAppliersMb.Length; i++)
                _slots[i] = _slotAppliersMb[i] as IWeaponStatsApplier;

            ApplyCurrent();
        }

        public void ApplyCurrent()
        {
            Debug.Log($"[APPLIER] weapons={_run.Weapons.Count}");

            for (int i = 0; i < _run.Weapons.Count; i++)
            {
                var w = _run.Weapons[i];
                Debug.Log($"[APPLIER] slot={i} id={w.Id} shape={w.Stats.Shape} dmg={w.Stats.Damage} range={w.Stats.Range} width={w.Stats.HitWidth} cd={w.Stats.Cooldown}");
            }

            if (_run == null) return;
            if (_run.Weapons == null || _run.Weapons.Count == 0)
            {
                Debug.LogError("[WeaponStatsApplier] No weapons in run");
                return;
            }

            if (_run == null)
            {
                Debug.LogError("[WeaponStatsApplier] RunContextService is null");
                return;
            }

            int weaponsCount = _run.Weapons.Count;

            for (int i = 0; i < _slots.Length; i++)
            {
                bool active = i < weaponsCount;

                if (_slotAppliersMb[i] != null)
                    _slotAppliersMb[i].enabled = active;

                if (!active)
                    continue;

                _slots[i]?.ApplyStats(_run.Weapons[i].Stats);
            }

            Debug.Log($"[APPLIER] weapons={weaponsCount}/{_slots.Length} " +
                      $"{string.Join(", ", _run.Weapons.Select(w => $"{w.Id}/{w.Stats.Shape}"))}");
        }
    }

}
    public interface IWeaponStatsApplier
    {
        void ApplyStats(WeaponStats stats);
    }
