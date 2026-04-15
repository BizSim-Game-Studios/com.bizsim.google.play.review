using System;
using System.Threading;
using System.Threading.Tasks;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Provider abstraction for the review flow. Consumers should use
    /// <see cref="ReviewController"/> rather than calling providers directly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The controller tracks in-flight state on its side (single sole-ownership per
    /// audit r4 B5); <see cref="IReviewProvider"/> intentionally does NOT carry an
    /// <c>IsRequestInFlight</c> property. Per CROSS-PACKAGE-INVARIANTS.md §12.2.1,
    /// the controller resolves the sentinel timeout (<c>-1f</c> → <c>DefaultTimeoutSeconds</c>)
    /// before delegating here — providers receive a positive <paramref name="timeoutSeconds"/>
    /// and use it directly.
    /// </para>
    /// </remarks>
    public interface IReviewProvider
    {
        event Action<ReviewResult> OnReviewFlowCompleted;
        event Action<ReviewError>  OnError;

        DateTime? LastPromptTimeUtc { get; }
        bool CanRequestReview();
        TimeSpan CooldownRemaining();

        void RequestReview();
        Task<ReviewResult> RequestReviewAsync(CancellationToken ct, float timeoutSeconds);

        // QA escape hatch — clears the local cooldown. Public so tests in a separate asmdef can call it.
        void ClearLocalCooldownForTesting();
    }
}
