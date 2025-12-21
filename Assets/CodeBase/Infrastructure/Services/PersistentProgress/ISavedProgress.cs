using CodeBase.Data;

namespace CodeBase.Hero
{
    public interface ISavedProgress
    {
        void UpdateProgress(PlayerProgress progress);
        void LoadProgress(PlayerProgress progress);
    }
}