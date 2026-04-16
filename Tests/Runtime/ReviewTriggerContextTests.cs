using System;
using NUnit.Framework;

namespace BizSim.Google.Play.Review.Tests
{
    [TestFixture]
    public class ReviewTriggerContextTests
    {
        [Test]
        public void Constructor_SetsAllFields()
        {
            var ctx = new ReviewTriggerContext(
                sessionCount: 5,
                launchCount: 12,
                daysSinceInstall: 14,
                lastFlowTimestamp: new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc),
                eventsRecorded: new[] { "level_completed", "purchase_made" },
                milestonesReached: new[] { "tutorial_done" },
                appVersion: "1.2.3"
            );

            Assert.AreEqual(5, ctx.SessionCount);
            Assert.AreEqual(12, ctx.LaunchCount);
            Assert.AreEqual(14, ctx.DaysSinceInstall);
            Assert.AreEqual(2, ctx.EventsRecorded.Count);
            Assert.AreEqual("1.2.3", ctx.AppVersion);
        }

        [Test]
        public void NullEvents_DefaultsToEmpty()
        {
            var ctx = new ReviewTriggerContext(0, 0, 0, DateTime.MinValue, null, null, "1.0");
            Assert.IsNotNull(ctx.EventsRecorded);
            Assert.AreEqual(0, ctx.EventsRecorded.Count);
        }
    }
}
