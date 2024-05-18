namespace Cat
{
    internal static partial class Commands
    {
        [CAspects.ConsumeException]
        internal static bool ViewLog()
        {
            FYI();
            return true;
        }
    }
}