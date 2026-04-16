# Architecture

Last reviewed: 2026-04-16

## Overview

The Review package follows the canonical BizSim Google Play bridge pattern: a Java bridge
on the Android side, a C# provider abstraction on the Unity side, and a MonoBehaviour
singleton controller that selects the provider at compile time.

## Component diagram

```
ReviewController (MonoBehaviour singleton)
    |
    +-- IReviewProvider (compile-time selection)
    |       |
    |       +-- AndroidReviewProvider (#if UNITY_ANDROID && !UNITY_EDITOR)
    |       |       |
    |       |       +-- ReviewCallbackProxy (AndroidJavaProxy)
    |       |       |       |
    |       |       |       +-- UnityMainThreadDispatcher.Enqueue()
    |       |       |
    |       |       +-- ReviewBridge.java (JNI entry point)
    |       |               |
    |       |               +-- ReviewManager (Play Core SDK)
    |       |
    |       +-- MockReviewProvider (Editor + non-Android)
    |               |
    |               +-- ReviewMockConfig (ScriptableObject)
    |
    +-- IReviewTriggerEngine (smart trigger evaluation)
    |
    +-- IConsentGate (GDPR consent check)
    |
    +-- IReviewAnalyticsAdapterV2 (optional telemetry)
    |
    +-- IFeedbackSink (optional feedback collection)
```

## Thread model

All public methods on `ReviewController` enforce main-thread execution via
`EnsureMainThread()`. Calling from a background thread throws
`InvalidOperationException`.

On the Android side, `ReviewBridge.java` posts all `ReviewManager` calls to
the main `Handler` (UI thread). Callbacks from Play Core arrive on the main
thread and are forwarded to C# via `ReviewCallbackProxy`, which uses
`UnityMainThreadDispatcher.Enqueue()` to marshal back to Unity's main thread.

## Provider selection

Provider selection happens at compile time, not at runtime:

- `#if UNITY_ANDROID && !UNITY_EDITOR` selects `AndroidReviewProvider`
- All other configurations select `MockReviewProvider`
- In Development Builds, `ReviewSettings.UseMockInDevelopmentBuild` can override
  to the mock provider even on Android

## Java bridge

`ReviewBridge.java` is a `public final class` with a synchronized singleton getter.
It receives the `Activity` reference in the constructor but creates the `ReviewManager`
from `activity.getApplicationContext()` so the manager survives activity recreation.

The `.androidlib` subproject (`BizSimReview.androidlib`) provides:
- `build.gradle` with `com.google.android.play:review:2.0.2`
- ProGuard keep rules for all JNI-accessed types
- A stub `AndroidManifest.xml`

## Cooldown and trigger engine

The `ReviewCoolDownLogic` class persists a `LastPromptTicks` timestamp in
`PlayerPrefs`. The `ReviewTriggerEngine` evaluates session count, days since
install, custom predicates, the kill switch, and the consent gate before
allowing a review request through to the provider.

## Data flow

1. Consumer calls `ReviewController.Instance.RequestReviewAsync()`
2. Trigger engine evaluates all conditions; returns early if blocked
3. Consent gate is checked; returns early if denied
4. Provider calls `ReviewBridge.requestReviewFlow()` (Android) or simulates (Mock)
5. Play Core shows (or suppresses) the review dialog
6. Callback fires; result is marshalled to C# main thread
7. Analytics adapter is notified; cooldown timestamp is persisted
