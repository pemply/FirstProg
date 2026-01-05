using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.Progress
{
    public class RunResetService :  IRunResetService
    {
        private readonly IPersistentProgressService _progress;
        private readonly IStaticDataService _staticData;

        public RunResetService(IPersistentProgressService progress, IStaticDataService staticData)
        {
            _progress = progress;
            _staticData = staticData;
        }

        public void ResetRunToDefaults()
        {
            var p = _progress.Progress;
            if (p == null)
            {
                Debug.LogError("[RunResetService] Progress is null");
                return;
            }

            p.RunProgressData = new RunProgressData();
            p.heroStats = new Stats();

            // XP/Level
            p.RunProgressData.Level = 1;
            p.RunProgressData.XpInLevel = 0;

            // Weapon defaults
            var weapon = _staticData.GetDefaultWeapon();
            if (weapon != null)
            {
                p.RunProgressData.WeaponId = weapon.WeaponId;
                p.RunProgressData.WeaponStats = weapon.BaseStats;
            }

            // Hero defaults (важливо: не 0, а стартові)
            // Якщо Stats має дефолти в конструкторі — ок.
            // Якщо ні — задай тут явно, як у твоєму NewProgress():
            // p.heroStats.MaxHP = 100;
            // p.heroStats.MoveSpeed = 5;
        }
    }
}