namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.ConsumeException]
        internal static bool StartProcessMeasuring()
        {
            FYI();
            return true;
        }
    }
}