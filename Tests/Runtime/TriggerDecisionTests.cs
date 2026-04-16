using NUnit.Framework;

namespace BizSim.Google.Play.Review.Tests
{
    [TestFixture]
    public class TriggerDecisionTests
    {
        [Test]
        public void Allow_IsAllow_ReturnsTrue()
        {
            var d = TriggerDecision.Allow;
            Assert.IsTrue(d.IsAllow);
            Assert.IsFalse(d.IsBlock);
            Assert.IsFalse(d.IsDefer);
        }

        [Test]
        public void Block_CarriesReason()
        {
            var d = TriggerDecision.Block("cooldown_active");
            Assert.IsTrue(d.IsBlock);
            Assert.AreEqual("cooldown_active", d.Reason);
        }

        [Test]
        public void Defer_CarriesMinDelay()
        {
            var d = TriggerDecision.Defer(System.TimeSpan.FromMinutes(5));
            Assert.IsTrue(d.IsDefer);
            Assert.AreEqual(5, d.MinDelay.TotalMinutes, 0.01);
        }

        [Test]
        public void Allow_Reason_IsNull()
        {
            Assert.IsNull(TriggerDecision.Allow.Reason);
        }
    }
}
