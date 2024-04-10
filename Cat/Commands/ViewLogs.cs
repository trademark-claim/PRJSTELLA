namespace Cat
{
    internal static partial class Commands
    {

        [LoggingAspects.ConsumeException]
        internal static bool ViewLog()
        {
            FYI();
            return true;
        }
    }
}