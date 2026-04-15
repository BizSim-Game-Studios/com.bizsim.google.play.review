using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Editor/non-Android mock configuration. Ships as ScriptableObject assets in
    /// <c>Samples~/MockPresets</c>. Drop one onto <c>ReviewController._mockConfig</c>
    /// (Inspector) or the mock provider's constructor to pick a scenario.
    /// </summary>
    [CreateAssetMenu(
        menuName = "BizSim/Google Play/Review/Mock Config",
        fileName = "ReviewMockConfig")]
    public sealed class ReviewMockConfig : ScriptableObject
    {
        [Tooltip("Delay before MockReviewProvider fires its completion/error callback. " +
                 "Zero means fire synchronously on the next frame.")]
        [Min(0f)]
        public float SimulatedDelaySeconds = 0.5f;

        [Tooltip("If NoError, the flow completes successfully. Otherwise, the provider " +
                 "fires OnError with this code (after SimulatedDelaySeconds).")]
        public ReviewErrorCode SimulatedError = ReviewErrorCode.NoError;

        [Tooltip("Logging-only: controls the info-level log prose that indicates whether " +
                 "the mock pretended to show a dialog. Does NOT change the returned result " +
                 "(Google's API is quota-invisible — we never expose 'was dialog shown?').")]
        public bool SimulateFlowShown = true;
    }
}
