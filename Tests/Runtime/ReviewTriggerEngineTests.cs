using NUnit.Framework;
using System;

namespace BizSim.Google.Play.Review.Tests
{
    [TestFixture]
    public class ReviewTriggerEngineTests
    {
        ReviewTriggerEngine _engine;
        TestConfigSource _config;

        [SetUp]
        public void Setup()
        {
            _config = new TestConfigSource();
            _engine = new ReviewTriggerEngine(
                _config, new AlwaysAllowConsentGate(),
                defaultMinSessions: 3, defaultMinDays: 7);
        }

        [Test]
        public void KillSwitch_Off_BlocksReview()
        {
            _config.RemoteEnabled = false;
            var ctx = MakeContext(sessions: 10, days: 30);
            var decision = _engine.Evaluate(ctx);
            Assert.IsTrue(decision.IsBlock);
            Assert.AreEqual("killswitch_disabled", decision.Reason);
        }

        [Test]
        public void ConsentGate_Denied_Blocks()
        {
            var engine = new ReviewTriggerEngine(
                _config, new DenyConsentGate(),
                defaultMinSessions: 0, defaultMinDays: 0);
            var ctx = MakeContext(sessions: 10, days: 30);
            var decision = engine.Evaluate(ctx);
            Assert.IsTrue(decision.IsBlock);
            Assert.AreEqual("consent_denied", decision.Reason);
        }

        [Test]
        public void OfflineGuard_WhenOffline_Blocks()
        {
            var engine = new ReviewTriggerEngine(
                _config, new AlwaysAllowConsentGate(),
                defaultMinSessions: 0, defaultMinDays: 0,
                offlineGuardEnabled: true,
                networkReachabilityProvider: () => false);
            var ctx = MakeContext(sessions: 10, days: 30);
            var decision = engine.Evaluate(ctx);
            Assert.IsTrue(decision.IsBlock);
            Assert.AreEqual("offline", decision.Reason);
        }

        [Test]
        public void FirstRunGrace_Blocks_WhenBelowDefaultThresholds()
        {
            var ctx = MakeContext(sessions: 1, days: 2);
            var decision = _engine.Evaluate(ctx);
            Assert.IsTrue(decision.IsBlock);
            Assert.AreEqual("first_run_grace", decision.Reason);
        }

        [Test]
        public void ConfigSource_MinSessionCount_Overrides_Default()
        {
            _config.MinSessionCount = 20;
            var ctx = MakeContext(sessions: 10, days: 30);
            var decision = _engine.Evaluate(ctx);
            Assert.IsTrue(decision.IsBlock);
            Assert.AreEqual("min_sessions_not_met", decision.Reason);
        }

        [Test]
        public void ConfigSource_MinLaunchCount_Checked()
        {
            _config.MinLaunchCount = 15;
            var ctx = MakeContext(sessions: 10, launches: 5, days: 30);
            var decision = _engine.Evaluate(ctx);
            Assert.IsTrue(decision.IsBlock);
            Assert.AreEqual("min_launches_not_met", decision.Reason);
        }

        [Test]
        public void RequiredEvents_NotMet_Blocks()
        {
            _config.RequiredEvents = new[] { "level_completed" };
            var ctx = MakeContext(sessions: 10, days: 30);
            var decision = _engine.Evaluate(ctx);
            Assert.IsTrue(decision.IsBlock);
            Assert.AreEqual("required_events_not_met", decision.Reason);
        }

        [Test]
        public void Allows_WhenAllConditionsMet()
        {
            var ctx = MakeContext(sessions: 10, days: 30);
            Assert.IsTrue(_engine.Evaluate(ctx).IsAllow);
        }

        [Test]
        public void Precedence_KillSwitch_BeforeConsent()
        {
            _config.RemoteEnabled = false;
            var engine = new ReviewTriggerEngine(
                _config, new DenyConsentGate(),
                defaultMinSessions: 0, defaultMinDays: 0);
            var ctx = MakeContext(sessions: 10, days: 30);
            Assert.AreEqual("killswitch_disabled", engine.Evaluate(ctx).Reason);
        }

        static ReviewTriggerContext MakeContext(int sessions, int days,
            int launches = -1, string[] events = null, string[] milestones = null) =>
            new(sessions, launches < 0 ? sessions : launches, days, DateTime.MinValue,
                events ?? Array.Empty<string>(), milestones ?? Array.Empty<string>(), "1.0");

        class TestConfigSource : IReviewConfigSource
        {
            public bool RemoteEnabled { get; set; } = true;
            public int? MinSessionCount { get; set; }
            public int? MinDaysSinceInstall { get; set; }
            public int? MinPromptIntervalDays { get; set; }
            public int? MinLaunchCount { get; set; }
            public string[] RequiredEvents { get; set; }
            public string[] RequiredMilestones { get; set; }
        }

        class DenyConsentGate : IConsentGate
        {
            public bool IsConsented(ReviewTriggerContext context) => false;
        }
    }
}
