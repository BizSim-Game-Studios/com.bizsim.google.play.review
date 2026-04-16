using System;

namespace BizSim.Google.Play.Review
{
    public static class ReviewPlayStoreFallback
    {
        static readonly System.Text.RegularExpressions.Regex PackageNameRegex =
            new(@"^[a-zA-Z][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*){1,}$",
                System.Text.RegularExpressions.RegexOptions.Compiled);

        static string ValidatePackageName(string packageName)
        {
            if (packageName == null) throw new ArgumentNullException(nameof(packageName));
            if (!PackageNameRegex.IsMatch(packageName))
                throw new ArgumentException(
                    $"Invalid Android package name: '{packageName}'. " +
                    "Must match [a-zA-Z][a-zA-Z0-9_]*(.[a-zA-Z][a-zA-Z0-9_]*)+",
                    nameof(packageName));
            return packageName;
        }

        public static string BuildMarketUri(string packageName) =>
            $"market://details?id={ValidatePackageName(packageName)}";

        public static string BuildPlayStoreWebUri(string packageName) =>
            $"https://play.google.com/store/apps/details?id={ValidatePackageName(packageName)}";

        public static void OpenPlayStoreListing(string packageName)
        {
            var uri = BuildMarketUri(packageName);
            #if UNITY_ANDROID && !UNITY_EDITOR
            using var intent = new UnityEngine.AndroidJavaObject(
                "android.content.Intent",
                "android.intent.action.VIEW",
                new UnityEngine.AndroidJavaClass("android.net.Uri")
                    .CallStatic<UnityEngine.AndroidJavaObject>("parse", uri));
            using var activity = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer")
                .GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
            activity.Call("startActivity", intent);
            #else
            UnityEngine.Application.OpenURL(uri);
            #endif
        }
    }
}
