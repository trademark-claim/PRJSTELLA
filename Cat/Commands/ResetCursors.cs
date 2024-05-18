namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        internal static bool ResetCursor()
        {
            Interface.AddLog("Resetting cursor...");
            BaselineInputs.Cursor.Reset();
            Interface.AddLog("Cursor Reset!");
            return true;
        }
    }
}