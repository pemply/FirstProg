namespace CodeBase.Infrastructure.States.BetweenStates
{
    public readonly struct ResumeWavesPayload
    {
        public readonly int WaveIndex;
        public ResumeWavesPayload(int waveIndex) => WaveIndex = waveIndex;
    }
}