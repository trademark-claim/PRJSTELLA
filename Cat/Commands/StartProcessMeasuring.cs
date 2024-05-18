namespace Cat
{
    internal static partial class Commands
    {
        private static List<Objects.ProcessManager> pms = [];
        internal static List<Objects.ProcessManager> PMs => pms;

        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool StartProcessMeasuring()
        {
            var ps = new Objects.ProcessSelector();
            ps.ShowDialog();
            new Objects.ProcessManager(ps.SelectedProcessId).Show();
            return true;
        }
    }
}