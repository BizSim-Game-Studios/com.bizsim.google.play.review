namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Optional sink for post-review textual feedback. Consumers inject an implementation
    /// via <see cref="ReviewController.SetFeedbackSink"/>. The default is
    /// <see cref="NullFeedbackSink"/> (no-op).
    /// </summary>
    /// <remarks>
    /// This is NOT wired to the Google Play review dialog (Google does not surface
    /// user-submitted review text to apps). It is a local-only capture mechanism for
    /// apps that show their own follow-up feedback UI after the review flow completes.
    /// </remarks>
    public interface IFeedbackSink
    {
        /// <summary>
        /// Called when the user submits optional textual feedback through the app's own UI.
        /// Implementations must not throw.
        /// </summary>
        void SubmitFeedback(string text);
    }
}
