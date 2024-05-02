namespace Cat
{
    internal static partial class Commands
    {
        private static List<Helpers.ProcessManager> pms = [];

        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        internal static bool StartProcessMeasuring()
        {
            FYI();
            return true;
        }
    }
}