using System.Collections.Generic;

namespace BizSim.Google.Play.Review.Editor
{
    /// <summary>
    /// Static validator that checks ProGuard / R8 rule content for the keep rules
    /// required by the Review JNI bridge. Called by <see cref="ReviewBuildValidator"/>
    /// at build time, and usable standalone in tests.
    /// </summary>
    public static class ReviewProguardValidator
    {
        /// <summary>
        /// Validates that <paramref name="proguardContent"/> contains the necessary
        /// keep rules for the Review bridge class and its nested callback interface.
        /// Returns an empty list when all rules are present.
        /// </summary>
        public static List<string> Validate(string proguardContent)
        {
            var warnings = new List<string>();
            if (string.IsNullOrEmpty(proguardContent))
            {
                warnings.Add(
                    "ProGuard rules file is empty. The Review bridge requires " +
                    "-keep rules for com.bizsim.google.play.review.ReviewBridge " +
                    "and its nested callback interfaces.");
                return warnings;
            }

            if (!proguardContent.Contains("com.bizsim.google.play.review.ReviewBridge"))
            {
                warnings.Add(
                    "Missing -keep rule for com.bizsim.google.play.review.ReviewBridge. " +
                    "Without this rule, R8/ProGuard may strip the JNI bridge class and " +
                    "cause NoClassDefFoundError at runtime.");
            }

            // The nested callback interface is referenced via $ notation in ProGuard.
            // Accept either ReviewBridge$IReviewCallback (exact) or ReviewBridge$* (wildcard).
            if (!proguardContent.Contains("ReviewBridge$IReviewCallback") &&
                !proguardContent.Contains("ReviewBridge$*"))
            {
                warnings.Add(
                    "Missing -keep rule for ReviewBridge nested callback interfaces " +
                    "(ReviewBridge$IReviewCallback or ReviewBridge$*). Without this rule, " +
                    "R8/ProGuard may strip the callback interface and cause " +
                    "NoSuchMethodError when the Java bridge invokes C# callbacks.");
            }

            return warnings;
        }
    }
}
