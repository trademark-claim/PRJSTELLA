namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        [CAspects.InDev]
        internal static bool SavePlot()
        {
            return true;
        }
    }
}