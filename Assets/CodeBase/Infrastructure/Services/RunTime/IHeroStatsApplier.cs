using CodeBase.Data;

namespace CodeBase.Infrastructure.Services.RunTime
{
    public interface  IHeroStatsApplier
    {
        void ApplyHeroStats(Stats stats);
    }
}