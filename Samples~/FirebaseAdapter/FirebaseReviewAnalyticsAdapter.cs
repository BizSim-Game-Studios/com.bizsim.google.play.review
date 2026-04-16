#if BIZSIM_FIREBASE
using Firebase.Analytics;
#endif

namespace BizSim.Google.Play.Review.Samples
{
    /// <summary>
    /// Sample <see cref="IReviewAnalyticsAdapterV2"/> implementation that logs all review
    /// events to Firebase Analytics. All events use the <c>bizsim_review_*</c> prefix.
    /// <para>
    /// <b>DO NOT RENAME</b> the event names — Firebase DebugView, BigQuery exports, and
    /// downstream dashboards depend on the exact strings. If you need different names,
    /// copy this file and modify the copy.
    /// </para>
    /// <para>
    /// <b>Policy note (P2):</b> This adapter intentionally does NOT log a
    /// <c>bizsim_review_submitted</c> event. Google's In-App Review API is
    /// quota-invisible — there is no way to know whether the user actually submitted
    /// a review. Logging such an event would be misleading.
    /// </para>
    /// </summary>
    public sealed class FirebaseReviewAnalyticsAdapter : IReviewAnalyticsAdapterV2
    {
        // ===================================================================
        // V1 methods (inherited from IReviewAnalyticsAdapter) — 4 methods
        // ===================================================================

        public void OnReviewRequested()
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_flow_requested");
#endif
        }

        public void OnReviewFlowCompleted(ReviewResult result)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_flow_completed",
                new Parameter("source", result.Source.ToString()),
                new Parameter("elapsed_ms", result.Elapsed.TotalMilliseconds));
#endif
        }

        public void OnReviewError(ReviewError error)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_error",
                new Parameter("error_code", error.Code.ToString()),
                new Parameter("retryable", error.Retryable ? "true" : "false"));
#endif
        }

        public void OnLocalCooldownCleared()
        {
            // Intentional no-op. Cooldown clearing is a QA action, not a user funnel event.
        }

        // ===================================================================
        // V2 context-carrying overloads of V1 events (B3 fix) — 3 methods
        // ===================================================================

        public void OnReviewRequested(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_flow_requested",
                new Parameter("session_count", ctx.SessionCount),
                new Parameter("trigger_reason", ctx.TriggerReason ?? ""),
                new Parameter("variant_id", ctx.VariantId ?? ""),
                new Parameter("days_since_install", ctx.DaysSinceInstall));
#endif
        }

        public void OnReviewFlowCompleted(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_flow_completed",
                new Parameter("session_count", ctx.SessionCount),
                new Parameter("trigger_reason", ctx.TriggerReason ?? ""),
                new Parameter("variant_id", ctx.VariantId ?? ""),
                new Parameter("days_since_install", ctx.DaysSinceInstall));
#endif
        }

        public void OnReviewError(ReviewError error, ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_error",
                new Parameter("error_code", error.Code.ToString()),
                new Parameter("retryable", error.Retryable ? "true" : "false"),
                new Parameter("session_count", ctx.SessionCount),
                new Parameter("trigger_reason", ctx.TriggerReason ?? ""));
#endif
        }

        // ===================================================================
        // V2 new events — 8 methods
        // ===================================================================

        public void OnTriggerEvaluated(TriggerDecision decision, ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_trigger_evaluated",
                new Parameter("decision", decision.Type.ToString()),
                new Parameter("reason", decision.Reason ?? ""),
                new Parameter("session_count", ctx.SessionCount),
                new Parameter("trigger_reason", ctx.TriggerReason ?? ""));
#endif
        }

        public void OnPreloadStarted(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_preload_started",
                new Parameter("session_count", ctx.SessionCount));
#endif
        }

        public void OnPreloadSucceeded(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_preload_succeeded",
                new Parameter("session_count", ctx.SessionCount));
#endif
        }

        public void OnPreloadFailed(ReviewError error, ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_preload_failed",
                new Parameter("error_code", error.Code.ToString()),
                new Parameter("session_count", ctx.SessionCount));
#endif
        }

        public void OnKillSwitchBlocked(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_killswitch_blocked",
                new Parameter("session_count", ctx.SessionCount),
                new Parameter("app_version", ctx.AppVersion ?? ""));
#endif
        }

        public void OnConsentBlocked(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_consent_blocked",
                new Parameter("session_count", ctx.SessionCount));
#endif
        }

        public void OnOfflineBlocked(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_offline_blocked",
                new Parameter("session_count", ctx.SessionCount));
#endif
        }

        public void OnCooldownBlocked(ReviewTelemetryContext ctx)
        {
#if BIZSIM_FIREBASE
            FirebaseAnalytics.LogEvent("bizsim_review_cooldown_blocked",
                new Parameter("session_count", ctx.SessionCount),
                new Parameter("days_since_install", ctx.DaysSinceInstall));
#endif
        }
    }
}
