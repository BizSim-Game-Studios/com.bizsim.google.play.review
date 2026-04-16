# Troubleshooting

Last reviewed: 2026-04-16

## 1. Review dialog never appears on device

**Problem:** `RequestReviewAsync` returns `FlowCompleted = true` but no dialog is visible.

**Cause:** Google Play silently suppresses the review dialog based on its own quota rules. `FlowCompleted = true` means the API call succeeded, not that the dialog was shown. This is by design (quota-invisible semantics).

**Fix:** This is expected behavior. Do not gate rewards or UI changes on whether the dialog was shown. Test with internal test tracks where quota is more lenient. Verify the app is installed from the Play Store (sideloaded APKs never show the dialog).

## 2. InvalidOperationException: Must be called on the main thread

**Problem:** Calling `ReviewController` methods from a background thread throws.

**Cause:** All public methods enforce main-thread execution via `EnsureMainThread()`.

**Fix:** Ensure you call `RequestReviewAsync` from a coroutine, `async void Start()`, or any code path running on Unity's main thread. If you must trigger from a background thread, use `UnityMainThreadDispatcher.Enqueue()` to marshal back first.

## 3. EDM4U fails to resolve com.google.android.play:review

**Problem:** Android build fails with missing dependency errors.

**Cause:** EDM4U has not resolved the Maven dependency declared in `Editor/Dependencies.xml`, or the OpenUPM scoped registry is missing from `manifest.json`.

**Fix:** Run **Assets > External Dependency Manager > Android Resolver > Force Resolve**. Verify that `Packages/manifest.json` contains the OpenUPM scoped registry with `com.google.external-dependency-manager` in its scopes array.

## 4. Mock provider always returns success in Editor

**Problem:** You want to test error paths but the mock provider always succeeds.

**Cause:** The default `ReviewMockConfig` simulates a successful flow.

**Fix:** Create a custom mock config via **Assets > Create > BizSim > Review Mock Config**. Set `SimulatedResult` to `Error` or `Timeout` and assign it to `ReviewController.MockConfig` in the Inspector.

## 5. Cooldown prevents testing

**Problem:** After one successful request, subsequent calls return immediately without reaching Play Core.

**Cause:** The 90-day local cooldown is active.

**Fix:** Call `ReviewController.Instance.ClearLocalCooldownForTesting()` from a debug menu or test script. This deletes the `BizSim.Review.LastPromptTicks` PlayerPrefs key.

## 6. Analytics events not appearing in Firebase

**Problem:** Firebase Analytics dashboard shows no `bizsim_review_*` events.

**Cause:** The analytics adapter is not registered, or `BIZSIM_FIREBASE` scripting define is missing.

**Fix:** Verify `BIZSIM_FIREBASE` is in your project's Scripting Define Symbols. Call `ReviewController.Instance.SetAnalyticsAdapter(new FirebaseReviewAnalyticsAdapter())` at startup, or enable `EnableAnalyticsByDefault` in ReviewSettings. Firebase events may take up to 24 hours to appear in the dashboard.

## 7. Trigger engine blocks all review requests

**Problem:** `RequestReviewAsync` returns without error but the trigger engine's `OnTriggerEvaluated` reports `Blocked`.

**Cause:** The session count or days-since-install thresholds have not been met, or the kill switch is enabled.

**Fix:** Check `ReviewSettings` for `MinSessionsBeforePrompt`, `MinDaysAfterInstall`, and `EnableKillSwitch`. Use `GetDiagnosticSnapshot()` to inspect the current trigger context values.
