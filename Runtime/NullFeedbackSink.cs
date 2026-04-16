namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Default no-op feedback sink. Used when no custom sink has been set via
    /// <see cref="ReviewController.SetFeedbackSink"/>.
    /// </summary>
    public sealed class NullFeedbackSink : IFeedbackSink
    {
        public static readonly NullFeedbackSink Instance = new();

        public void SubmitFeedback(string text)
        {
            // Intentional no-op. Override via ReviewController.SetFeedbackSink().
        }
    }
}
