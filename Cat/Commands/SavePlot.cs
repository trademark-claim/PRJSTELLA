namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.ConsumeException]
        internal static bool SavePlot()
        {
            FYI();
            return true;
        }
    }
}