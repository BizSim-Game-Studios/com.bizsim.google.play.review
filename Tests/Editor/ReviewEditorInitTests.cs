using NUnit.Framework;
using BizSim.Google.Play.Editor.Core;

namespace BizSim.Google.Play.Review.EditorTests
{
    public class ReviewEditorInitTests
    {
        [Test]
        public void InitializeOnLoad_RegistersReviewInstalledDefine()
        {
            foreach (var platform in BizSimDefineManager.GetRelevantPlatforms())
            {
                Assert.IsTrue(
                    BizSimDefineManager.IsDefinePresent("BIZSIM_REVIEW_INSTALLED", platform),
                    $"Expected BIZSIM_REVIEW_INSTALLED on {platform}");
            }
        }
    }
}
