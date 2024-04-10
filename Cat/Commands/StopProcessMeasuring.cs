namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.ConsumeException]
        internal static bool StopProcessMeasuring()
        {
            FYI();
            return true;
        }
    }
}