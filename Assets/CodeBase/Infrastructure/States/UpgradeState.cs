using System.Linq;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Infrastructure.States.BetweenStates;
using CodeBase.Logic.Upgrade;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class UpgradeState : IPayLoadedState<UpgradePayload>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IUpgradeService _upgrades;
        private readonly IStaticDataService _staticData;
        private readonly RunContextService _run;
        private readonly IPersistentProgressService _progress;
        public UpgradeState(
            GameStateMachine stateMachine,
            IUpgradeService upgrades,
            IStaticDataService staticData,
            RunContextService run, IPersistentProgressService progress)
        {
            _stateMachine = stateMachine;
            _upgrades = upgrades;
            _staticData = staticData;
            _run = run;
            _progress = progress;
        }
        

        public void Enter(UpgradePayload payload)
        {
            Time.timeScale = 0f;

            var hud = payload.Loop.Hud;

            var window = hud.GetComponentInChildren<UpgradeWindow>(true);
            if (window == null)
            {
                Debug.LogError("[UpgradeState] UpgradeWindow not found in HUD");
                Resume(payload.WaveIndex);
                return;
            }

            var pool = _staticData.AllUpgrades
                .Where(u => u != null && (u.MaxPicks <= 0 || _run.GetPicks(u.Type) < u.MaxPicks))
                .ToList();

            var options = UpgradeRandomPicker.Pick3(pool, _run, _staticData);

            // ⭐ ОЦЕ ГОЛОВНЕ: робимо "рол" значень + рідкість для UI і Apply
            float luck = _progress.Progress.heroStats.Luck;
            Debug.Log($"[UPGRADE] Hero Luck = {luck}");
            var rolls = UpgradeRoller.Roll3(options, _staticData.RarityChances, luck);


            window.Show(rolls, index =>
            {
                var roll = rolls[index];
                if (roll.Config != null)
                {
                    // якщо це "GetWeapon" — беремо previewId який уже в roll
                    if (roll.Config.Type == UpgradeType.GetSecondaryWeapon)
                        _run.PendingWeaponId = roll.WeaponPreviewId;

                    _upgrades.Apply(roll);
                }

                window.Hide();
                Resume(payload.WaveIndex);
            });
        }



        private void Resume(int waveIndex)
        {
            Time.timeScale = 1f;

            _stateMachine.Enter<GameLoopState, ResumeWavesPayload>(
                new ResumeWavesPayload(waveIndex)
            );
        }

        public void Exit() { }
    }
}
