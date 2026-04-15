#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// <see cref="AndroidJavaProxy"/> subclass implementing the Java interface
    /// <c>com.bizsim.google.play.review.ReviewBridge$IReviewCallback</c>. Method names and
    /// signatures MUST match the Java interface verbatim (case-sensitive). Any drift causes
    /// <c>NoSuchMethodError</c> at runtime.
    /// </summary>
    /// <remarks>
    /// All callbacks marshal through <see cref="UnityMainThreadDispatcher"/> so the invoking
    /// Action lambdas run on the Unity main thread, satisfying the main-thread-only contract
    /// documented on <see cref="IReviewAnalyticsAdapter"/> and the controller.
    /// </remarks>
    internal sealed class ReviewCallbackProxy : AndroidJavaProxy
    {
        private readonly Action _onCompleted;
        private readonly Action<int, string> _onError;

        public ReviewCallbackProxy(Action onCompleted, Action<int, string> onError)
            : base("com.bizsim.google.play.review.ReviewBridge$IReviewCallback")
        {
            _onCompleted = onCompleted;
            _onError = onError;
        }

        // Method names MUST match the Java interface EXACTLY (case-sensitive).
        public void onFlowCompleted()
            => UnityMainThreadDispatcher.Enqueue(() => _onCompleted?.Invoke());

        public void onError(int code, string message)
            => UnityMainThreadDispatcher.Enqueue(() => _onError?.Invoke(code, message));
    }
}
#endif
