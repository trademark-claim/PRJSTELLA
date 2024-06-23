namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        [CAspects.ConsumeException]
        [CAspects.InDev]
        internal static bool RemoveCursorFromPreset()
        {
            return true;
        }
    }
}