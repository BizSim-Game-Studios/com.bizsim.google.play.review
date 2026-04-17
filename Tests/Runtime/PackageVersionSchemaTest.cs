using NUnit.Framework;
using BizSim.Google.Play.Review;

namespace BizSim.Google.Play.Review.Tests
{
    /// <summary>
    /// Drift guard for K8 PackageVersion schema unification (Plan G).
    /// Per development-plans/plans/2026-04-17-enterprise-quality-bar/
    /// 06-conventions/06-package-version-schema.md.
    /// </summary>
    public class PackageVersionSchemaTest
    {
        [Test]
        public void NativeSdkFields_ArePopulated()
        {
            Assert.IsFalse(string.IsNullOrEmpty(PackageVersion.NativeSdkVersion));
            Assert.IsFalse(string.IsNullOrEmpty(PackageVersion.NativeSdkLabel));
            Assert.IsFalse(string.IsNullOrEmpty(PackageVersion.NativeSdkArtifactCoord));
        }

        [Test]
        public void NativeSdkArtifactCoord_EndsWithVersion()
        {
            Assert.IsTrue(PackageVersion.NativeSdkArtifactCoord.EndsWith(":" + PackageVersion.NativeSdkVersion),
                "NativeSdkArtifactCoord must end with ':' + NativeSdkVersion.");
        }

        [Test]
        public void NativeSdkFields_MatchExpectedReviewValues()
        {
            Assert.AreEqual("2.0.2", PackageVersion.NativeSdkVersion);
            Assert.AreEqual("Play Core (review)", PackageVersion.NativeSdkLabel);
            Assert.AreEqual("com.google.android.play:review:2.0.2", PackageVersion.NativeSdkArtifactCoord);
        }

#pragma warning disable CS0618
        [Test]
        public void LegacyAlias_ResolvesToSameValue()
        {
            Assert.AreEqual(PackageVersion.NativeSdkVersion, PackageVersion.PlayCoreVersion);
        }
#pragma warning restore CS0618
    }
}
