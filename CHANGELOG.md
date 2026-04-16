# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2026-04-16

### Added
- Smart trigger engine with session count, launch count, time gating, and event-driven hooks (`RecordEvent`, `RecordMilestone`)
- `IReviewConfigSource` interface for Firebase / UGS Remote Config with top-level `RemoteEnabled` kill switch
- `IConsentGate` interface for GDPR / DMA region shipments (default: `AlwaysAllowConsentGate`)
- `ReviewPlayStoreFallback` helper — error-only redirect to Play Store listing
- `ReviewController.GetDiagnosticSnapshot()` for support bundles
- `ReviewSessionTracker` with PlayerPrefs-backed session/launch counting
- Offline guard, in-flight watchdog (8s default), editor dry-run mode
- Editor Configuration tabs: Trigger Engine, Remote Config, Consent Gate, Diagnostics
- Build validator warnings for watchdog/timeout mismatch and dry-run in release builds
- Development-build warnings when kill switch or consent gate not wired (S4 security)

## [1.0.1] - 2026-04-15

### Fixed
- Relaxed runtime asmdef `includePlatforms` from `["Android", "Editor"]` to `[]`
  to fix a consumer-side `CS0246: The type or namespace name 'BizSim' could not
  be found` regression that appeared during Addressables content build on Android
  target. The Editor compile pass resolved the auto-reference correctly, but the
  Player script compile pass did not — a known Unity issue when `autoReferenced`
  library assemblies are platform-gated at the asmdef level.

  Runtime platform safety is preserved by the existing `#if UNITY_ANDROID && !UNITY_EDITOR`
  guards around every JNI call site; non-Android builds continue to route through
  `Mock<Api>Provider` per CROSS-PACKAGE-INVARIANTS §4.

  No API surface change. Consumers with existing `using BizSim.Google.Play.Review;`
  imports require no action — the fix is transparent on the next package install.

## [1.0.0] - 2026-04-15

### Added
- Initial release of the Google Play In-App Review bridge for Unity.
- `ReviewController` singleton with Task-based and event-based async APIs.
- Local 90-day cooldown via `ReviewCoolDownLogic` backed by PlayerPrefs.
- Mock provider + 6 ScriptableObject presets.
- Optional Firebase Analytics adapter guarded by `BIZSIM_FIREBASE`.
- Optional UniTask support guarded by `BIZSIM_UNITASK`.
- `editor.core` integration for Firebase define management.
- `BIZSIM_REVIEW_INSTALLED` define auto-registered at editor load.
