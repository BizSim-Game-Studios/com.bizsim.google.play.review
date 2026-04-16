using NUnit.Framework;

namespace BizSim.Google.Play.Review.Tests
{
    [TestFixture]
    public class ReviewPlayStoreFallbackTests
    {
        [Test]
        public void BuildMarketUri_ContainsPackageName()
        {
            var uri = ReviewPlayStoreFallback.BuildMarketUri("com.example.app");
            Assert.AreEqual("market://details?id=com.example.app", uri);
        }

        [Test]
        public void BuildMarketUri_NullPackage_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(
                () => ReviewPlayStoreFallback.BuildMarketUri(null));
        }

        [Test]
        public void BuildPlayStoreWebUri_ContainsPackageName()
        {
            var uri = ReviewPlayStoreFallback.BuildPlayStoreWebUri("com.example.app");
            StringAssert.Contains("com.example.app", uri);
            StringAssert.StartsWith("https://play.google.com/store/apps/details", uri);
        }
    }
}
