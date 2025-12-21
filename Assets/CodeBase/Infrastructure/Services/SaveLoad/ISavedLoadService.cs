using CodeBase.Data;

namespace CodeBase.Infrastructure.Services.SaveLoad
{
    public interface ISavedProgressService : IService
    {
        void SaveProgress();
        PlayerProgress LoadProgress();
    }
}