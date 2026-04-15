using System;

namespace BizSim.Google.Play.Review
{
    public enum ReviewErrorCode
    {
        // Mirrors com.google.android.play.core.review.model.ReviewErrorCode one-to-one.
        NoError            = 0,
        PlayStoreNotFound  = -1,
        InvalidRequest     = -2,
        InternalError      = -100,

        // BizSim extensions (do not collide with Google's int space — stay below -200).
        BridgeNotInitialized = -200,
        Timeout              = -201,
        CancelledByCaller    = -202,
        QuotaCooldownActive  = -203,  // Our local cooldown, not Google's.
        EditorMockError      = -204,  // Only from MockReviewProvider.
    }

    public enum ReviewFlowSource { Android, Mock }

    [Serializable]
    public readonly struct ReviewResult : IEquatable<ReviewResult>
    {
        public readonly bool FlowCompleted;        // True iff the launchReviewFlow Task completed successfully.
        public readonly DateTime CompletedAtUtc;
        public readonly TimeSpan Elapsed;
        public readonly ReviewFlowSource Source;   // Android or Mock — informational only.

        public ReviewResult(bool flowCompleted, DateTime completedAtUtc, TimeSpan elapsed, ReviewFlowSource source)
        {
            FlowCompleted = flowCompleted;
            CompletedAtUtc = completedAtUtc;
            Elapsed = elapsed;
            Source = source;
        }

        public bool Equals(ReviewResult other)
            => FlowCompleted == other.FlowCompleted
            && CompletedAtUtc == other.CompletedAtUtc
            && Elapsed == other.Elapsed
            && Source == other.Source;

        public override bool Equals(object obj) => obj is ReviewResult r && Equals(r);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = FlowCompleted ? 1 : 0;
                h = (h * 397) ^ CompletedAtUtc.GetHashCode();
                h = (h * 397) ^ Elapsed.GetHashCode();
                h = (h * 397) ^ (int)Source;
                return h;
            }
        }

        public static bool operator ==(ReviewResult a, ReviewResult b) => a.Equals(b);
        public static bool operator !=(ReviewResult a, ReviewResult b) => !a.Equals(b);

        // IMPORTANT: there is deliberately no "WasDialogShown" field.
        // Google's API does not expose that information. See 05-google-api-research.md §4.
    }

    [Serializable]
    public readonly struct ReviewError : IEquatable<ReviewError>
    {
        public readonly ReviewErrorCode Code;
        public readonly string Message;
        public readonly bool Retryable;
        public readonly DateTime OccurredAtUtc;

        public ReviewError(ReviewErrorCode code, string message, bool retryable, DateTime occurredAtUtc)
        {
            Code = code;
            Message = message ?? "";
            Retryable = retryable;
            OccurredAtUtc = occurredAtUtc;
        }

        public static bool IsRetryable(ReviewErrorCode code) =>
            code == ReviewErrorCode.InternalError ||
            code == ReviewErrorCode.Timeout ||
            code == ReviewErrorCode.BridgeNotInitialized;
        // PlayStoreNotFound and InvalidRequest are NOT retryable.
        // QuotaCooldownActive is not retryable — caller must wait.
        // BridgeNotInitialized IS retryable — caller retries after controller init completes.

        public bool Equals(ReviewError other)
            => Code == other.Code
            && Retryable == other.Retryable
            && OccurredAtUtc == other.OccurredAtUtc
            && string.Equals(Message, other.Message, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is ReviewError e && Equals(e);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = (int)Code;
                h = (h * 397) ^ (Retryable ? 1 : 0);
                h = (h * 397) ^ OccurredAtUtc.GetHashCode();
                h = (h * 397) ^ (Message?.GetHashCode() ?? 0);
                return h;
            }
        }

        public static bool operator ==(ReviewError a, ReviewError b) => a.Equals(b);
        public static bool operator !=(ReviewError a, ReviewError b) => !a.Equals(b);
    }
}
