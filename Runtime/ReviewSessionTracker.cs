using System;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    public sealed class ReviewSessionTracker
    {
        const string Prefix = "bizsim_review_";
        const string SessionKey = Prefix + "session_count";
        const string LaunchKey = Prefix + "launch_count";
        const string InstallKey = Prefix + "install_timestamp";

        public int SessionCount { get; private set; }
        public int LaunchCount { get; private set; }

        public int DaysSinceInstall
        {
            get
            {
                var ts = PlayerPrefs.GetString(InstallKey, "");
                if (string.IsNullOrEmpty(ts)) return 0;
                if (!long.TryParse(ts, out var ticks)) return 0;
                var install = new DateTime(ticks, DateTimeKind.Utc);
                return (int)(DateTime.UtcNow - install).TotalDays;
            }
        }

        public ReviewSessionTracker()
        {
            SessionCount = PlayerPrefs.GetInt(SessionKey, 0);
            LaunchCount = PlayerPrefs.GetInt(LaunchKey, 0);
            if (!PlayerPrefs.HasKey(InstallKey))
            {
                PlayerPrefs.SetString(InstallKey, DateTime.UtcNow.Ticks.ToString());
                PlayerPrefs.Save();
            }
        }

        public void RecordSession()
        {
            SessionCount++;
            PlayerPrefs.SetInt(SessionKey, SessionCount);
            PlayerPrefs.Save();
        }

        public void RecordLaunch()
        {
            LaunchCount++;
            PlayerPrefs.SetInt(LaunchKey, LaunchCount);
            PlayerPrefs.Save();
        }

        public bool IsInFirstRunGrace(int minSessions, int minDays) =>
            SessionCount < minSessions || DaysSinceInstall < minDays;
    }
}
