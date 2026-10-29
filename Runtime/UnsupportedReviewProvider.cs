using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Provider for players with no Play Store review API (iOS, standalone, WebGL). Shows nothing and
    /// fails every request with <see cref="ReviewErrorCode.PlayStoreNotFound"/>, like a device without Play Store.
    /// </summary>
    internal sealed class UnsupportedReviewProvider : IReviewProvider
    {
        public bool IsSupported => false;
        public event Action<ReviewResult> OnReviewFlowCompleted { add { } remove { } }
        public event Action<ReviewError> OnError;

        public DateTime? LastPromptTimeUtc => null;
        public bool CanRequestReview() => false;
        public TimeSpan CooldownRemaining() => TimeSpan.Zero;

        public void RequestReview() => Fail();

        public Task<ReviewResult> RequestReviewAsync(CancellationToken ct, float timeoutSeconds)
        {
            ReviewError error = Fail();
            throw new InvalidOperationException(error.Message);
        }

        public void ClearLocalCooldownForTesting() { }

        ReviewError Fail()
        {
            var error = new ReviewError(ReviewErrorCode.PlayStoreNotFound,
                "In-app review is not available on this platform.", retryable: false, occurredAtUtc: DateTime.UtcNow);
            OnError?.Invoke(error);
            return error;
        }
    }
}
