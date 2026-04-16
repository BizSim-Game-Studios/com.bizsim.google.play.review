# Firebase Analytics Adapter Sample

Drop-in `IReviewAnalyticsAdapterV2` implementation that logs all 15 review events to Firebase Analytics.

## Event names

All events use the `bizsim_review_*` prefix:

| Event | V1/V2 | Parameters |
|-------|-------|------------|
| `bizsim_review_flow_requested` | V1 + V2 | session_count, trigger_reason, variant_id, days_since_install |
| `bizsim_review_flow_completed` | V1 + V2 | source, elapsed_ms, session_count, trigger_reason, variant_id |
| `bizsim_review_error` | V1 + V2 | error_code, retryable, session_count, trigger_reason |
| `bizsim_review_trigger_evaluated` | V2 | decision, reason, session_count, trigger_reason |
| `bizsim_review_preload_started` | V2 | session_count |
| `bizsim_review_preload_succeeded` | V2 | session_count |
| `bizsim_review_preload_failed` | V2 | error_code, session_count |
| `bizsim_review_killswitch_blocked` | V2 | session_count, app_version |
| `bizsim_review_consent_blocked` | V2 | session_count |
| `bizsim_review_offline_blocked` | V2 | session_count |
| `bizsim_review_cooldown_blocked` | V2 | session_count, days_since_install |

**DO NOT RENAME** these event names. Firebase DebugView, BigQuery exports, and downstream dashboards depend on the exact strings. If you need different event names, copy this file into your project and modify the copy.

## Policy note

This adapter intentionally does NOT log a `bizsim_review_submitted` event. Google's In-App Review API is quota-invisible: `OnReviewFlowCompleted` firing does NOT mean the review dialog was shown, let alone that the user submitted a review. Logging a "submitted" event would be misleading and could lead to incorrect business metrics.

## Prerequisites

1. Firebase Unity SDK installed (`com.google.firebase.analytics`)
2. `BIZSIM_FIREBASE` scripting define active (auto-set via asmdef `versionDefines` when Firebase is installed)

## Usage

```csharp
// At app start, after Firebase initialization:
ReviewController.Instance.SetAnalyticsAdapter(new FirebaseReviewAnalyticsAdapter());
```

The controller calls V2 methods when the adapter implements `IReviewAnalyticsAdapterV2` and falls back to V1 parameterless calls for V1-only adapters. Since this adapter implements V2, both V1 and V2 events fire (V1 for backward compatibility, V2 for enriched context).
