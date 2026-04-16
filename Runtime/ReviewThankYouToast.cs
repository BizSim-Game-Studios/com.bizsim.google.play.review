using System;
using System.Collections;
using UnityEngine;

namespace BizSim.Google.Play.Review
{
    /// <summary>
    /// Optional post-review thank-you toast. When enabled via
    /// <see cref="ReviewSettings.ThankYouToastEnabled"/>, this helper displays a short
    /// Android toast message after the review flow completes.
    /// <para>
    /// On non-Android platforms and in the Editor, the toast is logged to the console
    /// instead. The message is not localizable by default — consumers who need i18n
    /// should disable this and show their own UI.
    /// </para>
    /// </summary>
    public static class ReviewThankYouToast
    {
        private const string DEFAULT_MESSAGE = "Thank you for your feedback!";
        private const float DEFAULT_DISPLAY_SECONDS = 3f;

        /// <summary>
        /// Shows a brief thank-you toast if the feature is enabled in ReviewSettings.
        /// Called by the controller after a successful review flow completion.
        /// </summary>
        public static void ShowIfEnabled(ReviewSettings settings)
        {
            if (settings == null || !settings.ThankYouToastEnabled) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                ShowAndroidToast(DEFAULT_MESSAGE);
            }
            catch (Exception ex)
            {
                BizSimLogger.Warning($"Thank-you toast failed: {ex.Message}");
            }
#else
            BizSimLogger.Info($"[ThankYouToast] {DEFAULT_MESSAGE}");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void ShowAndroidToast(string message)
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using var toastClass = new AndroidJavaClass("android.widget.Toast");
                using var toast = toastClass.CallStatic<AndroidJavaObject>(
                    "makeText", activity, message, 0 /* Toast.LENGTH_SHORT */);
                toast.Call("show");
            }));
        }
#endif
    }
}
