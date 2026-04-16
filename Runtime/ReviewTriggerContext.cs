using System;
using System.Collections.Generic;

namespace BizSim.Google.Play.Review
{
    public readonly struct ReviewTriggerContext
    {
        public int SessionCount { get; }
        public int LaunchCount { get; }
        public int DaysSinceInstall { get; }
        public DateTime LastFlowTimestamp { get; }
        public IReadOnlyList<string> EventsRecorded { get; }
        public IReadOnlyList<string> MilestonesReached { get; }
        public string AppVersion { get; }

        public ReviewTriggerContext(
            int sessionCount, int launchCount, int daysSinceInstall,
            DateTime lastFlowTimestamp, string[] eventsRecorded,
            string[] milestonesReached, string appVersion)
        {
            SessionCount = sessionCount;
            LaunchCount = launchCount;
            DaysSinceInstall = daysSinceInstall;
            LastFlowTimestamp = lastFlowTimestamp;
            EventsRecorded = eventsRecorded != null
                ? Array.AsReadOnly(eventsRecorded)
                : (IReadOnlyList<string>)Array.Empty<string>();
            MilestonesReached = milestonesReached != null
                ? Array.AsReadOnly(milestonesReached)
                : (IReadOnlyList<string>)Array.Empty<string>();
            AppVersion = appVersion ?? "";
        }
    }
}
