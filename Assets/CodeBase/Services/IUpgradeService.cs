using CodeBase.GameLogic.Upgrade;
using CodeBase.Infrastructure.Services;

namespace CodeBase.StaticData
{
    public interface IUpgradeService : IService
    {
        void Apply(UpgradeRoll roll);
    }
}