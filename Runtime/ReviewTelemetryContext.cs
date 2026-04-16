namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Immutable telemetry context passed to <see cref="IReviewAnalyticsAdapterV2"/> event methods.
    /// Captures the state at the moment of the event so analytics adapters can log enriched events
    /// without reaching back into the controller.
    /// </summary>
    public readonly struct ReviewTelemetryContext
    {
        /// <summary>Current application version (<c>Application.version</c>).</summary>
        public string AppVersion { get; }

        /// <summary>Days elapsed since first install (from <see cref="ReviewSessionTracker"/>).</summary>
        public int DaysSinceInstall { get; }

        /// <summary>Human-readable reason the review flow was triggered (e.g. "level_completed", "manual").</summary>
        public string TriggerReason { get; }

        /// <summary>Total sessions recorded via <see cref="ReviewController.RecordSession"/>.</summary>
        public int SessionCount { get; }

        /// <summary>Optional A/B test variant identifier. Null when not running an experiment.</summary>
        public string VariantId { get; }

        public ReviewTelemetryContext(
            string appVersion,
            int daysSinceInstall,
            string triggerReason,
            int sessionCount,
            string variantId)
        {
            AppVersion = appVersion ?? "";
            DaysSinceInstall = daysSinceInstall;
            TriggerReason = triggerReason ?? "";
            SessionCount = sessionCount;
            VariantId = variantId;
        }
    }
}
