using CodeBase.Infrastructure.Services;
using CodeBase.Logic.Upgrade;

namespace CodeBase.StaticData
{
    public interface IUpgradeService : IService
    {
        void Apply(UpgradeRoll roll);
    }
}