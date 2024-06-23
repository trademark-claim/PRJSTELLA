namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        [CAspects.ConsumeException]
        [CAspects.CDebug]
        internal static bool ThrowError()
        {
            int.Parse("hello there :3");
            return true;
        }
    }
}