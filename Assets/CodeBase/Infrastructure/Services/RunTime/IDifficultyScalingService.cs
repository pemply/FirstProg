namespace CodeBase.Infrastructure.Services.RunTime
{
    public interface IDifficultyScalingService : IService
    {
        int Tier { get; }
        float HpMult { get; }
        float DmgMult { get; }
        float XpMult { get; }

        void Tick(float elapsedSeconds);
        void Reset();
    }
}