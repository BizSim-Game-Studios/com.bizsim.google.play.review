# BizSim Google Play In-App Review Bridge

[![Unity 6000.0+](https://img.shields.io/badge/Unity-6000.0%2B-blue.svg)](https://unity.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
[![Version](https://img.shields.io/badge/Version-1.0.0-orange.svg)](CHANGELOG.md)

Unity bridge for the [Google Play In-App Review API](https://developer.android.com/guide/playcore/in-app-review) (v2.0.2).
Requests and launches the native in-app review flow with a local cooldown, analytics hooks, and an editor mock provider.

> **⚠️ Unofficial package.** This is a community-built Unity bridge for the Google Play In-App Review API. It is **not** an official Google product.

## Features

- **Java-to-C# Bridge** — Play Core `ReviewManager` wrapped in a main-thread-safe singleton
- **Task-based and event-based APIs** — `RequestReviewAsync()` + event callbacks fired on completion
- **Local cooldown** — 90-day PlayerPrefs-backed cooldown to respect Google's quota-invisible semantics
- **Mock provider** — 6 ScriptableObject presets covering success, error, and cooldown scenarios for editor + non-Android builds
- **Analytics adapter** — Optional `IReviewAnalyticsAdapter` with a Firebase implementation guarded by `BIZSIM_FIREBASE`
- **UniTask support** — Optional extension assembly guarded by `BIZSIM_UNITASK`
- **Editor integration** — Auto-registered `BIZSIM_REVIEW_INSTALLED` define via `editor.core`

## Installation

This package depends on Google's [External Dependency Manager for Unity (EDM4U)](https://github.com/googlesamples/unity-jar-resolver), which is published to the OpenUPM scoped registry. Add EDM4U's registry to your project's `Packages/manifest.json` once, then add this package as a Git URL — UPM will auto-install EDM4U on first import.

**Step 1 — Add the OpenUPM scoped registry (one-time per project):**

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

If you already have other OpenUPM-distributed packages, you may already have this registry — just add `com.google.external-dependency-manager` to the existing `scopes` array.

**Step 2 — Install this package via Git URL:**

```json
{
  "dependencies": {
    "com.bizsim.google.play.review": "https://github.com/BizSim-Game-Studios/com.bizsim.google.play.review.git#v1.0.0"
  }
}
```

After the package imports, EDM4U is automatically resolved by UPM — no manual `.unitypackage` import required. EDM4U then resolves the Android Maven dependency declared in `Editor/Dependencies.xml` (`com.google.android.play:review:2.0.2`) at the next Android build, or immediately via `Assets → External Dependency Manager → Android Resolver → Force Resolve`.

## Quick Start

1. Add `ReviewController` to a persistent GameObject (or access the `Instance` singleton).

2. Request the review flow:
   ```csharp
   var result = await ReviewController.Instance.RequestReviewAsync(CancellationToken.None, 30f);
   if (result.FlowCompleted)
   {
       // Google guarantees nothing about whether the dialog was shown.
       // Do NOT infer "user reviewed" from this signal.
   }
   ```

3. Local cooldown is managed automatically — `ReviewController` returns a cached `FlowCompleted` result during the 90-day cooldown window without calling Play Core.

## Quota-invisible semantics (IMPORTANT)

The Google Play In-App Review API is **quota-invisible**. This means:

- `FlowCompleted = true` does **not** mean the review dialog was shown. It only means the API call completed without error.
- Google Play silently suppresses the dialog based on its own quota rules, which are intentionally opaque.
- You **must not** gate rewards, prompts, or any user-visible behavior on "was the review shown?" — the answer is unknowable.

The local 90-day cooldown in this package is an additional safeguard layered on top of Google's own quota logic. Its purpose is to avoid spamming the `requestReviewFlow` call, not to track whether the user actually reviewed.

## Requirements

- Unity 6000.0 or later
- Android target platform
- **[EDM4U](https://github.com/googlesamples/unity-jar-resolver) (External Dependency Manager for Unity)** — auto-resolved via OpenUPM scoped registry (see Installation)
- Google Play In-App Review library 2.0.2 (resolved automatically via `Editor/Dependencies.xml`)

## Google Play Data Safety

### Data Collected

This package does **not** collect any user data. The Google Play In-App Review flow is rendered by the Play Store app in an overlay on your activity; any review text submitted by the user is sent directly from the Play Store app to Google's servers, never passing through your app.

The only persisted state is a single `long` timestamp in `PlayerPrefs` tracking the last time the review flow was requested (for the local cooldown).

### Data NOT Collected or Shared

- **No personal data** is collected by this package
- **No data is shared** with third parties
- **No network calls** are made by this package (Play Core handles all IPC to Google Play)

### Play Console Data Safety Form

When filling out the [Data Safety form](https://support.google.com/googleplay/android-developer/answer/10787469) in Google Play Console:

1. **Data types**: None collected by this package
2. **Collection purpose**: N/A
3. **Shared with third parties**: No

## License

This package's C# and Java source code is licensed under the [MIT License](LICENSE.md) — Copyright (c) 2026 BizSim Game Studios.

## Third-Party Licenses

This package does **not** bundle any Google SDK binaries. The native Android dependency is resolved at build time by [EDM4U](https://github.com/googlesamples/unity-jar-resolver) from the Google Maven repository (`maven.google.com`):

| Dependency | Version | License |
|-----------|---------|---------|
| `com.google.android.play:review` | 2.0.2 | [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0) |

For full third-party license details, see [NOTICES.md](NOTICES.md).
