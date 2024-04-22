namespace Cat
{
    internal static partial class Commands
    {
        internal static bool OpenLogEditor()
        {
            editor?.Close();
            editor = new LogEditor();
            editor.Show();
            Interface.AddLog("Opened Log Editor");
            return true;
        }
    }
}