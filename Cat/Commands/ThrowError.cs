namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        internal static bool ThrowError()
        {
            int.Parse("hello :3");
            return true;
        }
    }
}