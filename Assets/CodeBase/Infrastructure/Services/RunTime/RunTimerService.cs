namespace CodeBase.Infrastructure.Services.RunTime
{
    public class RunTimerService : IService
    {
        public float ElapsedSeconds { get; private set; }

        public void Reset() =>
            ElapsedSeconds = 0f;

        public void Tick(float deltaTime) =>
            ElapsedSeconds += deltaTime;
    }
}