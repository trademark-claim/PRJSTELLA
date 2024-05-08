namespace Cat
{
    internal static partial class Commands
    {
        internal static List<Objects.ProcessManager> PMs { get; } = [];

        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        internal static bool StartProcessMeasuring()
        {
            var ps = new Helpers.ProcessSelector();
            ps.ShowDialog();
            new Objects.ProcessManager(ps.SelectedProcessId).Show();
            Catowo.inst.ToggleInterface();
            return true;
        }
    }
}