using CodeBase.Data;
using CodeBase.Hero;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Infrastructure.Services.SaveLoad;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.States
{
    public class LoadProgressState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly IPersistentProgressService _progressService;
        private readonly ISavedLoadService _savedLoadService;
        private readonly IStaticDataService _staticData;

        public LoadProgressState(GameStateMachine gameStateMachine, IPersistentProgressService progressService, ISavedLoadService savedLoadService, IStaticDataService staticData)
        {
            _gameStateMachine = gameStateMachine;
            _progressService = progressService;
            _savedLoadService = savedLoadService;
            _staticData = staticData;
        }
        

        public void Enter()
        {
            LoadProgressOrInitNew();
            _gameStateMachine.Enter<LoadLevelState, string>(_progressService.Progress.WorldData.PositionOnLevel.Level);
        }

        private void LoadProgressOrInitNew()
        {
            _progressService.Progress = _savedLoadService.LoadProgress() ?? NewProgress();

            // repair for old/invalid saves
            if (_progressService.Progress.RunProgressData == null)
                _progressService.Progress.RunProgressData = new RunProgressData();

            if (_progressService.Progress.heroStats == null)
                _progressService.Progress.heroStats = new Stats();

            if (_progressService.Progress.WorldData == null)
                _progressService.Progress.WorldData = new WorldData("Main");

            RunProgressData run = _progressService.Progress.RunProgressData;
            var weapon = _staticData.GetDefaultWeapon();

            if (string.IsNullOrEmpty(run.WeaponId))
                run.WeaponId = weapon.WeaponId;

            if (run.WeaponStats.Damage <= 0 ||
                run.WeaponStats.DamageRadius <= 0 ||
                run.WeaponStats.BaseCooldown <= 0 ||
                run.WeaponStats.AttackSpeedMult <= 0)
            {
                run.WeaponStats = weapon.BaseStats;
            }


        }

        private PlayerProgress NewProgress()
        {
            var progress = new PlayerProgress(initialLevel: "Main");
            
            progress.RunProgressData ??= new RunProgressData();

            progress.RunProgressData.Level = 1;
            progress.RunProgressData.XpInLevel = 0;

            var weapon = _staticData.GetDefaultWeapon();
            progress.RunProgressData.WeaponId = weapon.WeaponId;
            progress.RunProgressData.WeaponStats = weapon.BaseStats;

            return progress;
        }

        public void Exit()
        {
            
        }
    }
}