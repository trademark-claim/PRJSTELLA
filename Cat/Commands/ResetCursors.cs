namespace Cat
{
    internal static partial class Commands
    {
        [LoggingAspects.ConsumeException]
        internal static bool ResetCursor()
        {
            Interface.AddLog("Resetting cursor...");
            BaselineInputs.Cursor.Reset();
            Interface.AddLog("Cursor Reset!");
            return true;
        }
    }
}