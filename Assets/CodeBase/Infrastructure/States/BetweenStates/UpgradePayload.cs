namespace CodeBase.Infrastructure.States.BetweenStates
{
    public readonly struct UpgradePayload
    {
        public readonly GameLoopPayload Loop;
        public readonly int WaveIndex;
        

        public UpgradePayload(GameLoopPayload loop, int waveIndex,  bool isReward =  false)
        {
            Loop = loop;
            WaveIndex = waveIndex;
        }
    }

}

