namespace CodeBase.Infrastructure.Services.Progress
{
    public struct XpChangedData
    {
        public readonly int Current;
        public readonly int Required;
        public readonly int Level;

        public XpChangedData(int current, int required, int level)
        {
            Current = current;
            Required = required;
            Level = level;
        }
    }
}