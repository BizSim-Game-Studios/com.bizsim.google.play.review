namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Extended analytics adapter interface (Wave 2). Inherits the 4 V1 methods from
    /// <see cref="IReviewAnalyticsAdapter"/> and adds context-carrying overloads of the
    /// V1 events plus 8 new decision-point events.
    /// <para>
    /// The controller dispatches V2 methods when the adapter implements this interface;
    /// V1-only adapters continue to receive the parameterless V1 calls. This extended-
    /// interface pattern is IL2CPP-safe (no default interface methods).
    /// </para>
    /// </summary>
    /// <remarks>
    /// Per ADR-006a: extended interface was chosen over default interface methods because
    /// Unity's IL2CPP backend (required on iOS and consoles) does not reliably support
    /// C# 8 default interface methods across all LTS versions in the support matrix.
    /// </remarks>
    public interface IReviewAnalyticsAdapterV2 : IReviewAnalyticsAdapter
    {
        // --- Context-carrying overloads of V1 events (B3 fix) ---
        // V1's OnReviewFlowCompleted() fires without telemetry context — the most important
        // event in the funnel has no session count, trigger reason, or variant ID.
        // V2 adds context-carrying overloads.

        /// <summary>Review flow was requested. Context-enriched V2 overload.</summary>
        void OnReviewRequested(ReviewTelemetryContext ctx);

        /// <summary>Review flow completed (quota-invisible). Context-enriched V2 overload.</summary>
        void OnReviewFlowCompleted(ReviewTelemetryContext ctx);

        /// <summary>Review flow error. Context-enriched V2 overload.</summary>
        void OnReviewError(ReviewError error, ReviewTelemetryContext ctx);

        // --- New V2 events (8 methods) ---

        /// <summary>Trigger engine evaluated and produced a decision (Allow, Block, or Defer).</summary>
        void OnTriggerEvaluated(TriggerDecision decision, ReviewTelemetryContext ctx);

        /// <summary>PreloadReviewInfo started fetching ReviewInfo from the provider.</summary>
        void OnPreloadStarted(ReviewTelemetryContext ctx);

        /// <summary>PreloadReviewInfo completed successfully; ReviewInfo is cached.</summary>
        void OnPreloadSucceeded(ReviewTelemetryContext ctx);

        /// <summary>PreloadReviewInfo failed with an error.</summary>
        void OnPreloadFailed(ReviewError error, ReviewTelemetryContext ctx);

        /// <summary>Review request blocked because the remote kill switch is disabled.</summary>
        void OnKillSwitchBlocked(ReviewTelemetryContext ctx);

        /// <summary>Review request blocked because user consent was not granted.</summary>
        void OnConsentBlocked(ReviewTelemetryContext ctx);

        /// <summary>Review request blocked because the device is offline.</summary>
        void OnOfflineBlocked(ReviewTelemetryContext ctx);

        /// <summary>Review request blocked because the local cooldown is still active.</summary>
        void OnCooldownBlocked(ReviewTelemetryContext ctx);
    }
}
