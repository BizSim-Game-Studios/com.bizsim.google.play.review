using NUnit.Framework;
using BizSim.Google.Play.Review;

namespace BizSim.Google.Play.Review.Tests
{
    public class BizSimLoggerPrefixTest
    {
        [Test]
        public void Prefix_IsExactlyBizSimReview()
        {
            Assert.AreEqual("[BizSim.Review] ", BizSimLogger.Prefix,
                "Per CROSS-PACKAGE-INVARIANTS.md §12.3, the per-package log prefix is a hard convention. Do not change.");
        }
    }
}
