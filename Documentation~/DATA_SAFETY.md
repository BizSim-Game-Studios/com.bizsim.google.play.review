# Data Safety Disclosure — BizSim Google Play In-App Review Bridge

This document is the source-of-truth for the package's Google Play Data Safety form answers. Consumers must fill out their own Play Console Data Safety form based on their full app's data practices; the entries below cover ONLY what this package adds.

## Data collected

**None.** The package does not collect user-identifying data. The Google Play In-App Review flow is rendered by the Play Store app in an overlay on your activity; any review text submitted by the user is sent directly from the Play Store app to Google's servers, never passing through your app's code.

## Data transmitted to Google

The Google Play Core library (`com.google.android.play:review:2.0.2`) handles all communication with the Play Store from its own process. This package's JNI bridge marshals only the `ReviewInfo` opaque handle and completion signals — no user-identifying data crosses the JNI boundary.

## Data transmitted to Firebase (if enabled)

The optional `IReviewAnalyticsAdapter` + its default `FirebaseReviewAnalyticsAdapter` implementation (guarded by the `BIZSIM_FIREBASE` scripting define) log **technical events only**:

- `bizsim_review_requested` — fires when `RequestReview()` / `RequestReviewAsync()` is called
- `bizsim_review_flow_completed` — parameters: `source` (Android/Mock), `elapsed_ms`
- `bizsim_review_error` — parameters: `code` (int), `retryable` (0/1)
- `bizsim_review_cooldown_cleared` — fires when `ClearLocalCooldownForTesting()` is called (QA only)

**No user identity, device identifier, ad ID, or quota state is transmitted.** The `code` parameter is the Google Play `ReviewErrorCode` int constant, not a user-facing string.

Consumers who enable Firebase must complete their own Play Console Data Safety form covering Firebase's data collection — this disclosure covers only what THIS package adds on top.

## Data persisted locally

**One PlayerPrefs key:** `BizSim.Review.LastPromptTicks`

- **Content:** a single `long` (`DateTime.UtcNow.Ticks`) — the timestamp of the last in-app review prompt request
- **Purpose:** local 90-day cooldown to protect against spamming `launchReviewFlow` (Google's quota-invisible protection is opaque to us)
- **Not PII:** it's a timestamp, not a user identifier
- **Scope:** per-device via Unity's `PlayerPrefs` (not synced to any cloud)
- **Clearance:** `ReviewController.Instance.ClearLocalCooldownForTesting()` (QA escape hatch; deletes the key)

## User controls

- **Local cooldown reset:** `ClearLocalCooldownForTesting()` — deletes the PlayerPrefs key. Intended for QA builds only; production builds should respect the cooldown.
- **Analytics opt-out:** consumers who want to disable the adapter call `ReviewController.Instance.SetAnalyticsAdapter(null)` (or never call `SetAnalyticsAdapter` — the default is no adapter).
- **Full package opt-out:** removing the package from `Packages/manifest.json` leaves only the PlayerPrefs key behind. Consumers who want a clean slate can delete the key from their app startup code: `PlayerPrefs.DeleteKey("BizSim.Review.LastPromptTicks")`.

## Play Console Data Safety form answers

When filling out the [Data Safety form](https://support.google.com/googleplay/android-developer/answer/10787469):

- **Does your app collect or share any of the required user data types?** — Not from this package alone. Answer based on your full app including Firebase/other SDKs.
- **Data types collected by this package:** None.
- **Data shared with third parties by this package:** None (Google Play Core talks to Google Play directly, not through your app).
- **Is the data encrypted in transit?** — N/A (this package doesn't transmit data).
- **Can users request their data be deleted?** — Yes, via `ClearLocalCooldownForTesting()` or manual `PlayerPrefs.DeleteKey`.

## References

- Package source: <https://github.com/BizSim-Game-Studios/com.bizsim.google.play.review>
- Google In-App Review API: <https://developer.android.com/guide/playcore/in-app-review>
- Play Console Data Safety: <https://support.google.com/googleplay/android-developer/answer/10787469>
- CROSS-PACKAGE-INVARIANTS.md §10 (shared template source)
