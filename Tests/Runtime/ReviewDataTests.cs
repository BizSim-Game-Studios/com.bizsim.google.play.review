using NUnit.Framework;
using BizSim.Google.Play.Review;

public class ReviewDataTests
{
    [TestCase(0, (int)ReviewErrorCode.NoError)]
    [TestCase(-1, (int)ReviewErrorCode.PlayStoreNotFound)]
    [TestCase(-2, (int)ReviewErrorCode.InvalidRequest)]
    [TestCase(-100, (int)ReviewErrorCode.InternalError)]
    public void ReviewErrorCode_MatchesGoogleIntValues(int expected, int actual)
        => Assert.AreEqual(expected, actual);

    [TestCase(ReviewErrorCode.InternalError, true)]
    [TestCase(ReviewErrorCode.Timeout, true)]
    [TestCase(ReviewErrorCode.BridgeNotInitialized, true)]
    [TestCase(ReviewErrorCode.PlayStoreNotFound, false)]
    [TestCase(ReviewErrorCode.InvalidRequest, false)]
    [TestCase(ReviewErrorCode.QuotaCooldownActive, false)]
    public void ReviewError_IsRetryable_MatchesPolicy(ReviewErrorCode code, bool expected)
        => Assert.AreEqual(expected, ReviewError.IsRetryable(code));

    [Test]
    public void ReviewResult_HasNoWasDialogShownField()
    {
        // Regression: Google's API does not expose this, so neither do we.
        var fields = typeof(ReviewResult).GetFields();
        Assert.That(fields, Has.None.Matches<System.Reflection.FieldInfo>(f =>
            f.Name.ToLowerInvariant().Contains("wasshown") ||
            f.Name.ToLowerInvariant().Contains("dialogshown")));
    }
}
