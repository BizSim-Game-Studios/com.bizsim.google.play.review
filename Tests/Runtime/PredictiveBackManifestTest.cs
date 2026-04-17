using System.IO;
using NUnit.Framework;

namespace BizSim.Google.Play.Review.Tests
{
    /// <summary>
    /// C5.2 drift guard (Plan E). Asserts the .androidlib manifest explicitly
    /// declares android:enableOnBackInvokedCallback, per the per-package table
    /// in development-plans/plans/2026-04-17-enterprise-quality-bar/
    /// 06-conventions/05-predictive-back-audit.md.
    ///
    /// For review, expected value is "true" — Play Core's review dialog is a
    /// system-managed Activity overlay that handles predictive-back internally.
    /// </summary>
    public class PredictiveBackManifestTest
    {
        private const string ManifestPath =
            "Packages/com.bizsim.google.play.review/Runtime/Plugins/Android/BizSimReview.androidlib/AndroidManifest.xml";

        private const string FallbackPath =
            "Runtime/Plugins/Android/BizSimReview.androidlib/AndroidManifest.xml";

        private static string ReadManifest()
        {
            if (File.Exists(ManifestPath))
                return File.ReadAllText(ManifestPath);
            if (File.Exists(FallbackPath))
                return File.ReadAllText(FallbackPath);
            Assert.Inconclusive("Could not locate AndroidManifest.xml at either " +
                ManifestPath + " or " + FallbackPath);
            return null;
        }

        [Test]
        public void Manifest_DeclaresPredictiveBackCallback_True()
        {
            var xml = ReadManifest();
            Assert.IsTrue(xml.Contains("enableOnBackInvokedCallback=\"true\""),
                "Per C5.2 bar convention, review's .androidlib manifest must declare " +
                "android:enableOnBackInvokedCallback=\"true\". See " +
                "development-plans/plans/2026-04-17-enterprise-quality-bar/" +
                "06-conventions/05-predictive-back-audit.md.");
        }
    }
}
