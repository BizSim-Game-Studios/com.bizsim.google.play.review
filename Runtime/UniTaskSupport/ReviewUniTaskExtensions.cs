#if BIZSIM_UNITASK
using Cysharp.Threading.Tasks;
using System.Threading;

namespace BizSim.Google.Play.Review.UniTask
{
    /// <summary>
    /// UniTask adapter for <see cref="IReviewProvider"/>. Consumers who pull UniTask into their
    /// project automatically get `RequestReviewUniTask()` on any provider.
    /// </summary>
    /// <remarks>
    /// Post-freeze r5: this extension takes the RESOLVED timeout from the caller. Consumers who
    /// want sentinel resolution (`-1f` → DefaultTimeoutSeconds) must call through
    /// <c>ReviewController.RequestReviewAsync</c> which does the sentinel dance; the provider-level
    /// extension expects a positive value per the IReviewProvider contract (§12.2.1).
    /// </remarks>
    public static class ReviewUniTaskExtensions
    {
        public static async UniTask<ReviewResult> RequestReviewUniTask(
            this IReviewProvider p, CancellationToken ct, float timeoutSeconds)
        {
            return await p.RequestReviewAsync(ct, timeoutSeconds);
        }
    }
}
#endif
