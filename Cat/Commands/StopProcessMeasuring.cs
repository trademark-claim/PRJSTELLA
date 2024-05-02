namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.ConsumeException]
        internal static bool StopProcessMeasuring()
        {
            int? entry = commandstruct.Value.Parameters[0][0] as int?;
            if (entry == null)
            {
                
            }
        }
    }
}