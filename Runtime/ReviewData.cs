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

    [System.Serializable]
    public readonly struct ReviewResult
    {
        public readonly bool FlowCompleted;        // True iff the launchReviewFlow Task completed successfully.
        public readonly System.DateTime CompletedAtUtc;
        public readonly System.TimeSpan Elapsed;
        public readonly ReviewFlowSource Source;   // Android or Mock — informational only.

        public ReviewResult(bool flowCompleted, System.DateTime completedAtUtc, System.TimeSpan elapsed, ReviewFlowSource source)
        {
            FlowCompleted = flowCompleted;
            CompletedAtUtc = completedAtUtc;
            Elapsed = elapsed;
            Source = source;
        }

        // IMPORTANT: there is deliberately no "WasDialogShown" field.
        // Google's API does not expose that information. See 05-google-api-research.md §4.
    }

    [System.Serializable]
    public readonly struct ReviewError
    {
        public readonly ReviewErrorCode Code;
        public readonly string Message;
        public readonly bool Retryable;
        public readonly System.DateTime OccurredAtUtc;

        public ReviewError(ReviewErrorCode code, string message, bool retryable, System.DateTime occurredAtUtc)
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
    }
}
