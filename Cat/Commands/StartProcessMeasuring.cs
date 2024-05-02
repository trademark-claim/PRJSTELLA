namespace Cat
{
    internal static partial class Commands
    {
        private static List<Helpers.ProcessManager> pms = [];

        [LoggingAspects.Logging]
        [LoggingAspects.ConsumeException]
        internal static bool StartProcessMeasuring()
        {
            var ps = new Helpers.ProcessSelector();
            ps.ShowDialog();
            new Helpers.ProcessManager(ps.SelectedProcessId).Show();
            return true;
        }
    }
}