namespace Cat
{
    internal static partial class Commands
    {
        /// <summary>
        /// Initiates a test to generate a progressing test sequence.
        /// </summary>
        /// <returns>Always returns true, indicating the method has completed execution.</returns>
        /// <remarks>
        /// This method is used to trigger a progress test, useful for debugging or demonstration purposes.
        /// </remarks>
        [LoggingAspects.ConsumeException]
        internal static bool GPT()
        {
            Helpers.ProgressTesting.GenerateProgressingTest();
            return true;
        }
    }
}