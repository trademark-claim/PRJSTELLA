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

        [CAspects.Logging]
        [CAspects.AsyncExceptionSwallower]
        internal static async Task TStartProcessMeasuring()
        {
            ClaraHerself.Custom = [
                "Command description:\n\""
            + (string)Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["close log editor"]
                ]["desc"]
            + "\"",
            "This command opens a new process measurer, just run it and it'll prompt you to enter the process you want to being measuing, and it'll automatically begin!",
            "Lets try it by running 'spm'"
            ];
            ClaraHerself.RunClara(ClaraHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await ClaraHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("spm");
        }
    }
}