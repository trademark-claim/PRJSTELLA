namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.Logging]
        internal static bool CLE()
        {
            editor?.Close();
            Interface.AddLog("Closed!");
            Logging.Log("Editor closed!");
            return true;
        }
    }
}