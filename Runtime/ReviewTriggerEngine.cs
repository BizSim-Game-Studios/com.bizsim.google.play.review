using System;
using System.Linq;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    public sealed class ReviewTriggerEngine : IReviewTriggerEngine
    {
        readonly IReviewConfigSource _config;
        readonly IConsentGate _consentGate;
        readonly int _defaultMinSessions;
        readonly int _defaultMinDays;
        readonly bool _offlineGuardEnabled;
        readonly Func<bool> _networkReachabilityProvider;

        public ReviewTriggerEngine(
            IReviewConfigSource config,
            IConsentGate consentGate,
            int defaultMinSessions = 3,
            int defaultMinDays = 7,
            bool offlineGuardEnabled = true,
            Func<bool> networkReachabilityProvider = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _consentGate = consentGate ?? throw new ArgumentNullException(nameof(consentGate));
            _defaultMinSessions = defaultMinSessions;
            _defaultMinDays = defaultMinDays;
            _offlineGuardEnabled = offlineGuardEnabled;
            _networkReachabilityProvider = networkReachabilityProvider
                ?? (() => Application.internetReachability != NetworkReachability.NotReachable);
        }

        public TriggerDecision Evaluate(ReviewTriggerContext context)
        {
            if (!_config.RemoteEnabled)
                return TriggerDecision.Block("killswitch_disabled");

            if (!_consentGate.IsConsented(context))
                return TriggerDecision.Block("consent_denied");

            if (_offlineGuardEnabled && !_networkReachabilityProvider())
                return TriggerDecision.Block("offline");

            if (context.SessionCount < _defaultMinSessions
                || context.DaysSinceInstall < _defaultMinDays)
                return TriggerDecision.Block("first_run_grace");

            // S3 security fix: clamp negative config values to null (skip)
            // to prevent misconfigured Remote Config from silently disabling thresholds
            var minSessions = ClampNonNegative(_config.MinSessionCount, "MinSessionCount");
            if (minSessions.HasValue && context.SessionCount < minSessions.Value)
                return TriggerDecision.Block("min_sessions_not_met");

            var minLaunches = ClampNonNegative(_config.MinLaunchCount, "MinLaunchCount");
            if (minLaunches.HasValue && context.LaunchCount < minLaunches.Value)
                return TriggerDecision.Block("min_launches_not_met");

            var minDays = ClampNonNegative(_config.MinDaysSinceInstall, "MinDaysSinceInstall");
            if (minDays.HasValue && context.DaysSinceInstall < minDays.Value)
                return TriggerDecision.Block("min_days_not_met");

            var required = _config.RequiredEvents;
            if (required != null && required.Length > 0
                && !required.All(e => context.EventsRecorded.Contains(e)))
                return TriggerDecision.Block("required_events_not_met");

            var requiredMilestones = _config.RequiredMilestones;
            if (requiredMilestones != null && requiredMilestones.Length > 0
                && !requiredMilestones.All(m => context.MilestonesReached.Contains(m)))
                return TriggerDecision.Block("required_milestones_not_met");

            return TriggerDecision.Allow;
        }

        static int? ClampNonNegative(int? value, string fieldName)
        {
            if (!value.HasValue) return null;
            if (value.Value < 0)
            {
                BizSimLogger.Warning(
                    $"IReviewConfigSource.{fieldName} returned {value.Value} (negative). " +
                    "Treating as null (skip). Check your Remote Config setup.");
                return null;
            }
            return value;
        }
    }
}
