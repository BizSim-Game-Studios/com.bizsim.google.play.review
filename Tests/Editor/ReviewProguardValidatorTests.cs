using NUnit.Framework;

namespace BizSim.Google.Play.Review.Editor.Tests
{
    [TestFixture]
    public class ReviewProguardValidatorTests
    {
        [Test]
        public void Validate_MissingKeepRule_ReturnsWarning()
        {
            var proguardContent = "# empty proguard file";
            var warnings = ReviewProguardValidator.Validate(proguardContent);
            Assert.IsTrue(warnings.Count > 0);
            StringAssert.Contains("ReviewBridge", warnings[0]);
        }

        [Test]
        public void Validate_CorrectRules_ReturnsEmpty()
        {
            var proguardContent = @"
-keep class com.bizsim.google.play.review.ReviewBridge { *; }
-keep class com.bizsim.google.play.review.ReviewBridge$* { *; }
";
            var warnings = ReviewProguardValidator.Validate(proguardContent);
            Assert.AreEqual(0, warnings.Count);
        }

        [Test]
        public void Validate_ExactCallbackInterface_ReturnsEmpty()
        {
            var proguardContent = @"
-keep class com.bizsim.google.play.review.ReviewBridge { *; }
-keep class com.bizsim.google.play.review.ReviewBridge$IReviewCallback { *; }
";
            var warnings = ReviewProguardValidator.Validate(proguardContent);
            Assert.AreEqual(0, warnings.Count);
        }

        [Test]
        public void Validate_NullContent_ReturnsWarning()
        {
            var warnings = ReviewProguardValidator.Validate(null);
            Assert.IsTrue(warnings.Count > 0);
            StringAssert.Contains("empty", warnings[0]);
        }

        [Test]
        public void Validate_EmptyString_ReturnsWarning()
        {
            var warnings = ReviewProguardValidator.Validate("");
            Assert.IsTrue(warnings.Count > 0);
            StringAssert.Contains("empty", warnings[0]);
        }

        [Test]
        public void Validate_BridgeOnly_MissingCallback_ReturnsOneWarning()
        {
            var proguardContent = @"
-keep class com.bizsim.google.play.review.ReviewBridge { *; }
";
            var warnings = ReviewProguardValidator.Validate(proguardContent);
            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains("callback", warnings[0].ToLowerInvariant());
        }

        [Test]
        public void Validate_CallbackOnly_MissingBridge_ReturnsOneWarning()
        {
            var proguardContent = @"
-keep class com.bizsim.google.play.review.ReviewBridge$IReviewCallback { *; }
";
            var warnings = ReviewProguardValidator.Validate(proguardContent);
            Assert.AreEqual(1, warnings.Count);
            StringAssert.Contains("ReviewBridge", warnings[0]);
        }
    }
}
