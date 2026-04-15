using NUnit.Framework;
using UnityEditor;
using BizSim.Google.Play.Review.Editor;

namespace BizSim.Google.Play.Review.EditorTests
{
    public class ReviewConfigurationTests
    {
        [Test]
        public void ReviewConfiguration_OpensWithoutException()
        {
            var window = EditorWindow.GetWindow<ReviewConfiguration>();
            Assert.NotNull(window);
            window.Close();
        }
    }
}
