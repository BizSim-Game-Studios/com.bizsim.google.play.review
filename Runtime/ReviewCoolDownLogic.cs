using System;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Local 90-day cooldown tracker for in-app review prompts. Backed by PlayerPrefs.
    /// Defensive against corrupt values, clock skew, and disk-full save failures.
    /// </summary>
    public sealed class ReviewCoolDownLogic
    {
        private const string PREF_KEY = "BizSim.Review.LastPromptTicks";
        private const string VERSION_KEY = "BizSim.Review.LastPromptAppVersion";
        private readonly int _minIntervalDays;

        public ReviewCoolDownLogic(int minIntervalDays)
        {
            if (minIntervalDays < 1)
                throw new ArgumentOutOfRangeException(nameof(minIntervalDays),
                    minIntervalDays, "minIntervalDays must be >= 1 to avoid an always-open cooldown.");
            _minIntervalDays = minIntervalDays;
        }

        // Defensive read: handles missing key, empty string, corrupt value, out-of-range ticks.
        public DateTime? LastPromptTimeUtc
        {
            get
            {
                if (!PlayerPrefs.HasKey(PREF_KEY)) return null;
                var raw = PlayerPrefs.GetString(PREF_KEY);
                if (string.IsNullOrEmpty(raw)) return null;
                if (!long.TryParse(raw, out var ticks)) return null;
                try { return new DateTime(ticks, DateTimeKind.Utc); }
                catch (ArgumentOutOfRangeException) { return null; }
            }
        }

        // Clock-skew safe: if the stored timestamp is in the FUTURE (user rolled back
        // device clock, or save-file restore from a newer version), treat it as "just now"
        // so we keep the cooldown active rather than allowing an immediate re-prompt.
        public bool CanRequestReview()
        {
            var last = LastPromptTimeUtc;
            if (last == null) return true;
            var now = DateTime.UtcNow;
            if (last.Value > now) return false;               // clock-skew: in future → still on cooldown
            return (now - last.Value).TotalDays >= _minIntervalDays;
        }

        // Clock-skew safe: if elapsed is negative, return the full interval (conservative).
        public TimeSpan Remaining()
        {
            var last = LastPromptTimeUtc;
            if (last == null) return TimeSpan.Zero;
            var interval = TimeSpan.FromDays(_minIntervalDays);
            var elapsed = DateTime.UtcNow - last.Value;
            if (elapsed < TimeSpan.Zero) return interval;     // clock-skew clamp
            return elapsed >= interval ? TimeSpan.Zero : interval - elapsed;
        }

        // PlayerPrefs.Save() can silently fail on disk-full / permission issues.
        // We wrap in try/catch and log; the cooldown is advisory and recovers on next StampNow.
        public void StampNow()
        {
            try
            {
                PlayerPrefs.SetString(PREF_KEY, DateTime.UtcNow.Ticks.ToString());
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                // Per CROSS-INVARIANTS §12.4 sub-tag policy, BizSimLogger.Prefix is the only tag — no `[ReviewCoolDown]` sub-bracket.
                BizSimLogger.Warning($"PlayerPrefs.Save failed in ReviewCoolDownLogic.StampNow: {ex.Message}. Cooldown not persisted for this call.");
            }
        }

        public void ClearForTesting()
        {
            PlayerPrefs.DeleteKey(PREF_KEY);
            PlayerPrefs.DeleteKey(VERSION_KEY);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Resets the cooldown if the current app version differs from the version stored
        /// at the last prompt. Called at controller Awake when <see cref="ReviewSettings.ResetCooldownOnVersionChange"/>
        /// is true.
        /// </summary>
        /// <returns>True if the cooldown was reset due to a version change.</returns>
        public bool ResetIfVersionChanged(string currentAppVersion)
        {
            if (string.IsNullOrEmpty(currentAppVersion)) return false;
            var stored = PlayerPrefs.GetString(VERSION_KEY, "");
            if (string.IsNullOrEmpty(stored))
            {
                // First run or key never set — store current version, no reset needed.
                try
                {
                    PlayerPrefs.SetString(VERSION_KEY, currentAppVersion);
                    PlayerPrefs.Save();
                }
                catch (Exception ex)
                {
                    BizSimLogger.Warning($"PlayerPrefs.Save failed in ResetIfVersionChanged (first run): {ex.Message}");
                }
                return false;
            }

            if (stored == currentAppVersion) return false;

            // Version changed — clear cooldown and store new version.
            BizSimLogger.Info($"App version changed ({stored} -> {currentAppVersion}): resetting review cooldown.");
            try
            {
                PlayerPrefs.DeleteKey(PREF_KEY);
                PlayerPrefs.SetString(VERSION_KEY, currentAppVersion);
                PlayerPrefs.Save();
            }
            catch (Exception ex)
            {
                BizSimLogger.Warning($"PlayerPrefs.Save failed in ResetIfVersionChanged: {ex.Message}");
            }
            return true;
        }
    }
}
