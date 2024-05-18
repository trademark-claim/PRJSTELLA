namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        internal static bool StopProcessMeasuring()
        {
            int? entry = commandstruct.Value.Parameters[0][0] as int?;
            if (entry == null)
            {
                
            }
            return true;
        }
    }
}