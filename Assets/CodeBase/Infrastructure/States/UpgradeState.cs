using CodeBase.Infrastructure.States.BetweenStates;
using CodeBase.Logic.Upgrade;
using CodeBase.StaticData;
using CodeBase.UI;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class UpgradeState :IPayLoadedState<UpgradePayload>
    {
        private readonly GameStateMachine _stateMachine;
        private readonly IUpgradeService _upgrades;
        private readonly IStaticDataService _staticData;

        public UpgradeState(GameStateMachine stateMachine, IUpgradeService upgrades, IStaticDataService staticData)
        {
            _stateMachine = stateMachine;
            _upgrades = upgrades;
            _staticData = staticData;
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

            var choices = UpgradeRandomPicker.Pick3(_staticData.AllUpgrades);

            window.Show(choices, index =>
            {
                var cfg = choices[index];
                if (cfg != null)
                    _upgrades.Apply(cfg);

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