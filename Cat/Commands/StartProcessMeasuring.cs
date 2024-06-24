namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// List of currently open process managers
        /// </summary>
        private static List<Objects.ProcessManager> pms = [];
        /// <summary>
        /// Abstraction property for <c><see cref="pms"/></c>
        /// </summary>
        internal static List<Objects.ProcessManager> PMs => pms;

        /// <summary>
        /// Command to start tracking a process
        /// </summary>
        /// <returns></returns>
        [CAspects.Logging]
        [CAspects.ConsumeException]
        internal static bool StartProcessMeasuring()
        {
            var ps = new Objects.ProcessSelector();
            ps.ShowDialog();
            var pm = new ProcessManager(ps.SelectedProcessId);
            pms.Add(pm);
            pm.Show();
            return true;
        }

        /// <summary>
        /// Tutorial for the SPM command
        /// </summary>
        /// <returns></returns>
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
                    .cmdmap["spm"]
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