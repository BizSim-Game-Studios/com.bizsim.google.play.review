using NUnit.Framework;
using UnityEngine;

namespace BizSim.Google.Play.Review.Tests
{
    [TestFixture]
    public class ReviewSessionTrackerTests
    {
        const string KeyPrefix = "bizsim_review_";

        [SetUp]
        public void ClearPrefs()
        {
            PlayerPrefs.DeleteKey(KeyPrefix + "session_count");
            PlayerPrefs.DeleteKey(KeyPrefix + "launch_count");
            PlayerPrefs.DeleteKey(KeyPrefix + "install_timestamp");
        }

        [Test]
        public void RecordSession_IncrementsSessionCount()
        {
            var tracker = new ReviewSessionTracker();
            tracker.RecordSession();
            tracker.RecordSession();
            Assert.AreEqual(2, tracker.SessionCount);
        }

        [Test]
        public void RecordLaunch_IncrementsLaunchCount()
        {
            var tracker = new ReviewSessionTracker();
            tracker.RecordLaunch();
            Assert.AreEqual(1, tracker.LaunchCount);
        }

        [Test]
        public void DaysSinceInstall_ReturnsZero_OnFirstRun()
        {
            var tracker = new ReviewSessionTracker();
            Assert.AreEqual(0, tracker.DaysSinceInstall);
        }

        [Test]
        public void IsInFirstRunGrace_True_WhenBelowThresholds()
        {
            var tracker = new ReviewSessionTracker();
            Assert.IsTrue(tracker.IsInFirstRunGrace(minSessions: 3, minDays: 7));
        }

        [Test]
        public void IsInFirstRunGrace_False_WhenSessionsExceedThreshold()
        {
            var tracker = new ReviewSessionTracker();
            for (int i = 0; i < 5; i++) tracker.RecordSession();
            Assert.IsFalse(tracker.IsInFirstRunGrace(minSessions: 3, minDays: 0));
        }
    }
}
