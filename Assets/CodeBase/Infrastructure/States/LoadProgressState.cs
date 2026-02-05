using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class LoadProgressState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly ISavedLoadService _savedLoadService;

        public LoadProgressState(
            GameStateMachine gameStateMachine,
            IPersistentProgressService progressService,
            ISavedLoadService savedLoadService)
        {
            _gameStateMachine = gameStateMachine;
            _progressService = progressService;
            _savedLoadService = savedLoadService;
        }

        public void Enter()
        {
            Debug.Log("[FLOW] LoadProgress.Enter");

            LoadProgressOrInitNew();
            _gameStateMachine.Enter<LoadLevelState, string>(_progressService.Progress.WorldData.PositionOnLevel.Level);
        }

        private void LoadProgressOrInitNew()
        {
            _progressService.Progress = _savedLoadService.LoadProgress() ?? NewProgress();
            RepairProgress(_progressService.Progress);
        }

        private PlayerProgress NewProgress() =>
            new PlayerProgress(initialLevel: "Main");

        private void RepairProgress(PlayerProgress p)
        {
            p.heroStats ??= new Stats();
            p.WorldData ??= new WorldData("Main");
            p.KillData ??= new KillData();
        }

        public void Exit() { }
    }
}