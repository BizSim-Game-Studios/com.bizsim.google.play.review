# Upgrade Guide: BizSim Google Play In-App Review Bridge 1.x

This guide covers each minor release in the 1.x series and what you need to
change (if anything) when upgrading.

---

## 1.0.0 / 1.0.1 to 1.1.0 (Enterprise Wave 1)

### What changed

1.1.0 adds the **smart trigger engine**, **remote kill switch**, **consent gate**,
**diagnostics**, and several build-time safety checks. All new APIs are
**additive** -- existing 1.0.x consumers compile and run without changes.

### New interfaces and defaults

| Interface | Default implementation | Purpose |
|---|---|---|
| `IReviewTriggerEngine` | `ReviewTriggerEngine` | Session/day/event-driven gating |
| `IReviewConfigSource` | `StaticReviewConfigSource` (always enabled) | Remote kill switch + remote parameter overrides |
| `IConsentGate` | `AlwaysAllowConsentGate` | GDPR/DMA consent check |

### New types

- `ReviewTriggerContext` -- immutable context struct passed to trigger engine and consent gate
- `TriggerDecision` -- value type with `Allow`, `Block(reason)`, `Defer(delay)` factory methods
- `ReviewSessionTracker` -- PlayerPrefs-backed session/launch/install-date counter
- `ReviewPlayStoreFallback` -- static helper to open Play Store listing on hard errors
- `ReviewDiagnosticSnapshot` -- serializable snapshot for support bundles

### New ReviewSettings fields

`FirstRunGraceSessions`, `FirstRunGraceDays`, `WatchdogTimeoutSeconds`,
`OfflineGuardEnabled`, `DryRunMode`

### New ReviewController APIs

- `SetTriggerEngine(IReviewTriggerEngine)` -- override the default trigger engine
- `SetConfigSource(IReviewConfigSource)` -- plug Firebase Remote Config or UGS Remote Config
- `SetConsentGate(IConsentGate)` -- plug your CMP (consent management platform)
- `RecordEvent(string)` / `RecordMilestone(string)` -- feed data to the trigger engine
- `RecordSession()` -- increment the session counter
- `GetDiagnosticSnapshot()` -- capture current state for debugging

### Easy path (zero overrides)

If you do not need remote config, GDPR gating, or custom triggers:

```csharp
// Nothing changes. ReviewController.Instance.RequestReview() works as before.
// The default StaticReviewConfigSource keeps the kill switch ON.
// The default AlwaysAllowConsentGate never blocks.
// The default ReviewTriggerEngine applies the grace period from ReviewSettings.
```

### Advanced path (Firebase wiring)

```csharp
void Start()
{
    var controller = ReviewController.Instance;

    // Remote kill switch via Firebase Remote Config
    controller.SetConfigSource(new MyFirebaseConfigSource());

    // GDPR consent gate
    controller.SetConsentGate(new MyCmpConsentGate());

    // Feed session data for trigger gating
    controller.RecordSession();
}
```

---

## 1.1.0 to 1.2.0 (Enterprise Wave 2)

### What changed

1.2.0 adds **preload caching**, **V2 analytics adapter**, **feedback sink**,
**per-version cooldown reset**, and a **thank-you toast**. All changes are
**additive** -- 1.1.0 consumers compile and run without modification.

### New interfaces and defaults

| Interface | Default implementation | Purpose |
|---|---|---|
| `IReviewAnalyticsAdapterV2` | (none -- opt-in) | Extended analytics with telemetry context |
| `IFeedbackSink` | `NullFeedbackSink` (no-op) | Post-review negative-experience routing |

### New types

- `ReviewTelemetryContext` -- immutable struct carrying version, days, trigger reason, session count, variant ID
- `ReviewThankYouToast` -- optional Android toast after flow completion
- `NullFeedbackSink` -- default no-op feedback sink

### New ReviewSettings fields

`ResetCooldownOnVersionChange`, `ThankYouToastEnabled`

### New ReviewController APIs

- `PreloadReviewInfo()` -- pre-fetch ReviewInfo with 5-min TTL
- `IsPreloadCached` -- check if preloaded info is still valid
- `RequestReview(string triggerReason, string variantId)` -- telemetry-enriched overload
- `RequestReviewAsync(string triggerReason, string variantId, ...)` -- async telemetry-enriched overload
- `SetFeedbackSink(IFeedbackSink)` -- plug a post-review feedback handler
- `SubmitFeedback(string)` -- submit feedback through the configured sink

### Easy path (zero overrides)

```csharp
// Nothing changes. All V2 features are opt-in.
// If you set a V1 IReviewAnalyticsAdapter, it continues to work.
// The V2 context-carrying events only fire if your adapter implements
// IReviewAnalyticsAdapterV2.
```

### Advanced path (Firebase wiring)

```csharp
void Start()
{
    var controller = ReviewController.Instance;

    // V2 analytics adapter with full telemetry context
    controller.SetAnalyticsAdapter(new FirebaseReviewAnalyticsAdapter());

    // Preload for faster launch
    controller.PreloadReviewInfo();

    // Trigger with context
    controller.RequestReview(triggerReason: "level_completed", variantId: "experiment_42");
}
```

### Firebase Adapter sample

Import the **Firebase Analytics Adapter** sample from Package Manager. It ships
a complete `IReviewAnalyticsAdapterV2` implementation that logs all 15 events
to Firebase Analytics with the `bizsim_review_*` prefix. Copy and modify as
needed.

---

## 1.2.0 to 1.3.0 (Enterprise Wave 3 -- Hardening)

### What changed

1.3.0 is a **hardening release**. No new runtime interfaces. Changes:

- **ProGuard build-time validator** -- `ReviewProguardValidator` checks the
  consumer's `proguard-user.txt` for missing keep rules at Android build time
- **Trigger presets** -- three `ReviewSettings` ScriptableObject presets
  (CasualGameTrigger, HardcoreGameTrigger, UtilityAppTrigger) shipped as a sample
- **`WriteDiagnosticSnapshot(path)`** -- writes the diagnostic snapshot to a
  JSON file (development builds and editor only; release builds return false)
- **Documentation** -- UPGRADE-1.x.md (this file), TELEMETRY-DASHBOARD.md

### Easy path

No code changes needed. Import the Trigger Presets sample if you want pre-tuned
settings for your app category.

---

## Deprecation notes

No APIs have been deprecated in the 1.x series. All changes are strictly additive.

---

## Quick reference: all new interfaces across 1.x

| Version | Interface | Default | Required? |
|---|---|---|---|
| 1.1.0 | `IReviewTriggerEngine` | `ReviewTriggerEngine` | No -- auto-created |
| 1.1.0 | `IReviewConfigSource` | `StaticReviewConfigSource` | No -- kill switch always ON |
| 1.1.0 | `IConsentGate` | `AlwaysAllowConsentGate` | No -- always consented |
| 1.2.0 | `IReviewAnalyticsAdapterV2` | (none) | No -- opt-in via `SetAnalyticsAdapter` |
| 1.2.0 | `IFeedbackSink` | `NullFeedbackSink` | No -- opt-in via `SetFeedbackSink` |
