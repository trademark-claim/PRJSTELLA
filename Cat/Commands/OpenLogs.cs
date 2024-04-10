namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.ConsumeException]
        internal static bool OpenLogs()
        {
            FYI();
            return true;
        }
    }
}