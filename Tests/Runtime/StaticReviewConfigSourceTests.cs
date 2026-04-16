using NUnit.Framework;

namespace BizSim.Google.Play.Review.Tests
{
    [TestFixture]
    public class StaticReviewConfigSourceTests
    {
        [Test]
        public void RemoteEnabled_DefaultsToTrue()
        {
            var src = new StaticReviewConfigSource();
            Assert.IsTrue(src.RemoteEnabled);
        }

        [Test]
        public void Thresholds_ReturnNull_FallingBackToSettings()
        {
            var src = new StaticReviewConfigSource();
            Assert.IsNull(src.MinSessionCount);
            Assert.IsNull(src.MinDaysSinceInstall);
            Assert.IsNull(src.MinPromptIntervalDays);
        }
    }
}
