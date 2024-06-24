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
            StellaHerself.Custom = [
                "Command description:\n\""
            + Interface.
                CommandProcessing
                .Cmds[Interface
                    .CommandProcessing
                    .cmdmap["close log editor"]
                ].desc
            + "\"",
            "This command opens a new process measurer, just run it and it'll prompt you to enter the process you want to being measuing, and it'll automatically begin!",
            "Lets try it by running 'spm'"
            ];
            StellaHerself.RunStella(StellaHerself.Mode.Custom, Catowo.inst.canvas);
            var b = await StellaHerself.TCS.Task;
            if (!b) return;
            Interface.CommandProcessing.ProcessCommand("spm");
        }
    }
}