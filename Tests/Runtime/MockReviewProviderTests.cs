using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BizSim.Google.Play.Review;

namespace BizSim.Google.Play.Review.Tests
{
    public class MockReviewProviderTests
    {
        private static ReviewMockConfig MakeConfig(float delay, ReviewErrorCode err)
        {
            var c = ScriptableObject.CreateInstance<ReviewMockConfig>();
            c.SimulatedDelaySeconds = delay;
            c.SimulatedError = err;
            c.SimulateFlowShown = true;
            return c;
        }

        [SetUp]
        public void SetUp() => PlayerPrefs.DeleteKey("BizSim.Review.LastPromptTicks");

        [TearDown]
        public void TearDown() => PlayerPrefs.DeleteKey("BizSim.Review.LastPromptTicks");

        [UnityTest]
        public IEnumerator RequestReview_FiresCompleted_OnSuccess()
        {
            var cd = new ReviewCoolDownLogic(90);
            var cfg = MakeConfig(0.1f, ReviewErrorCode.NoError);
            var p = new MockReviewProvider(cfg, cd);
            bool completed = false;
            p.OnReviewFlowCompleted += _ => completed = true;
            p.RequestReview();
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(completed);
        }

        [UnityTest]
        public IEnumerator RequestReview_FiresError_OnSimulatedError()
        {
            var cd = new ReviewCoolDownLogic(90);
            var cfg = MakeConfig(0.0f, ReviewErrorCode.PlayStoreNotFound);
            var p = new MockReviewProvider(cfg, cd);
            ReviewError? err = null;
            p.OnError += e => err = e;
            p.RequestReview();
            yield return null;
            yield return null;
            Assert.NotNull(err);
            Assert.AreEqual(ReviewErrorCode.PlayStoreNotFound, err.Value.Code);
        }

        [Test]
        public async Task RequestReviewAsync_ThrowsOnCancellation()
        {
            var cd = new ReviewCoolDownLogic(90);
            var cfg = MakeConfig(1f, ReviewErrorCode.NoError);
            var p = new MockReviewProvider(cfg, cd);
            var cts = new CancellationTokenSource();
            var task = p.RequestReviewAsync(cts.Token, 10f);
            cts.Cancel();
            try { await task; Assert.Fail("expected OperationCanceledException"); }
            catch (System.OperationCanceledException) { /* pass */ }
        }

        [UnityTest]
        public IEnumerator RequestReview_RespectsCooldown()
        {
            var cd = new ReviewCoolDownLogic(90);
            var cfg = MakeConfig(0.0f, ReviewErrorCode.NoError);
            var p = new MockReviewProvider(cfg, cd);
            bool gotError = false;
            ReviewError capturedError = default;
            p.OnError += e => { gotError = true; capturedError = e; };
            cd.StampNow();  // Activate cooldown
            p.RequestReview();
            yield return null;
            yield return null;
            Assert.IsTrue(gotError);
            Assert.AreEqual(ReviewErrorCode.QuotaCooldownActive, capturedError.Code);
        }
    }
}
