namespace CodeBase.GameLogic.Pool
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}