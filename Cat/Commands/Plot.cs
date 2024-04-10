namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.ConsumeException]
        [LoggingAspects.Logging]
        internal static bool Plot()
        {
            FYI();
            return true;
        }
    }
}