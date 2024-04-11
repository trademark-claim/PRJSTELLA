namespace Cat
{
    internal static partial class Commands
    {
        internal static bool OpenLogEditor() //Command
        {

            new LogEditor().Show();
            Interface.AddLog("Opened Log Editor");
            return true;
        }
    }
}