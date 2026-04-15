namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Optional telemetry adapter. Consumer plugs this into the controller via
    /// <c>SetAnalyticsAdapter</c>. All methods are fired on the Unity main thread.
    /// Implementations MUST NOT throw — the controller wraps calls in try/catch but
    /// a repeated throw pollutes the Unity console with warnings.
    /// </summary>
    public interface IReviewAnalyticsAdapter
    {
        void OnReviewRequested();
        void OnReviewFlowCompleted(ReviewResult result);
        void OnReviewError(ReviewError error);
        void OnLocalCooldownCleared();
    }
}
