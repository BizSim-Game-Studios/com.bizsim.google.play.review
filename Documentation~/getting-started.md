# Getting Started

Last reviewed: 2026-04-16

## Prerequisites

- Unity 6000.0 or later
- Android build target selected in Build Settings
- EDM4U (External Dependency Manager for Unity) installed via OpenUPM scoped registry

## Step 1 — Install the package

Add the OpenUPM scoped registry to your project's `Packages/manifest.json` if not already present:

```json
{
  "scopedRegistries": [
    {
      "name": "package.openupm.com",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.google.external-dependency-manager"
      ]
    }
  ]
}
```

Then add the package dependency:

```json
{
  "dependencies": {
    "com.bizsim.google.play.review": "https://github.com/BizSim-Game-Studios/com.bizsim.google.play.review.git#v1.3.0"
  }
}
```

## Step 2 — Resolve Android dependencies

After the package imports, run **Assets > External Dependency Manager > Android Resolver > Force Resolve**. This pulls `com.google.android.play:review:2.0.2` from Google Maven.

## Step 3 — Open the Configuration window

Navigate to **BizSim > Google Play > Review > Configuration** in the Unity menu bar. Verify that the Settings asset is created at `Assets/Resources/BizSim/GooglePlay/ReviewSettings.asset`. Adjust log level and cooldown defaults as needed.

## Step 4 — Request a review

Add the following to any MonoBehaviour in your project:

```csharp
using BizSim.Google.Play.Review;
using System.Threading;
using UnityEngine;

public class ReviewExample : MonoBehaviour
{
    async void Start()
    {
        var result = await ReviewController.Instance.RequestReviewAsync(CancellationToken.None, 30f);
        if (result.FlowCompleted)
            Debug.Log("Review flow completed (dialog may or may not have been shown).");
    }
}
```

## Step 5 — Verify in Editor

Enter Play Mode. The mock provider returns a simulated successful flow by default. Check the Console for `[BizSim.Review]` log entries confirming the mock path was taken.

## Step 6 — Test on a device

Build and deploy to an Android device with a Play Store account. The review dialog is only shown for apps distributed through the Play Store. Internal test tracks work for testing.

## What to expect

- `FlowCompleted = true` means the API call succeeded, NOT that the dialog was shown. Google's quota logic is opaque by design.
- The local 90-day cooldown prevents repeated calls. Use `ClearLocalCooldownForTesting()` in QA builds to reset it.
- The trigger engine evaluates session count, days since install, and custom predicates before allowing a review request.
