using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public class DifficultyScalingService : IDifficultyScalingService
    {
        private readonly DifficultyConfig _cfg;

        private float _nextUpgradeTime;

        public int Tier { get; private set; }

        public float HpMult =>
            Mathf.Min(_cfg.BaseHpMult * (1f + _cfg.HpStep * Tier), _cfg.MaxHpMult);

        public float DmgMult =>
            Mathf.Min(_cfg.BaseDmgMult * (1f + _cfg.DmgStep * Tier), _cfg.MaxDmgMult);

        public float XpMult =>
            Mathf.Min(_cfg.BaseXpMult * (1f + _cfg.XpStep * Tier), _cfg.MaxXpMult);

        public DifficultyScalingService(DifficultyConfig cfg)
        {
            _cfg = cfg;
            Tier = 0;
            _nextUpgradeTime = _cfg.FirstUpgradeAfterSeconds;
        }

        public void Tick(float elapsedSeconds)
        {
            while (Tier < _cfg.MaxTier && elapsedSeconds >= _nextUpgradeTime)
            {
                Tier++;
                _nextUpgradeTime += _cfg.UpgradeEverySeconds;
            }
        }

        public void Reset()
        {
            Tier = 0;
            _nextUpgradeTime = _cfg.FirstUpgradeAfterSeconds;
        }
    }
}