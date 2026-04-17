# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.4.2] - 2026-04-17

### Fixed
- **Three-point version drift hotfix.** v1.4.1 shipped with `PackageVersion.Current` bumped to `"1.4.1"` but `package.json.version` + `CHANGELOG.md` top section still at `1.4.0` (tool-call race in the release commit). v1.4.1 tag is published but internally inconsistent. v1.4.2 restores three-point consistency. **Consumers on 1.4.1 should upgrade to 1.4.2.**
- C5.2 predictive-back manifest declaration and `PredictiveBackManifestTest` drift guard documented in v1.4.1 remain in place (unchanged by this hotfix).

## [1.4.0] - 2026-04-17

### Added
- **K8 PackageVersion schema unification (Plan G).** Three new `public const string` fields on `PackageVersion`: `NativeSdkVersion` (`"2.0.2"`), `NativeSdkLabel` (`"Play Core (review)"`), `NativeSdkArtifactCoord` (`"com.google.android.play:review:2.0.2"`). Enables workspace-wide consistent native SDK version reporting in `BizSimPackageDashboard` and the `version-drift-check.sh` hook. See `development-plans/plans/2026-04-17-enterprise-quality-bar/06-conventions/06-package-version-schema.md`.
- `PackageVersionSchemaTest` drift guard (4 assertions).

### Deprecated
- `PackageVersion.PlayCoreVersion` — now an `[Obsolete]` alias of `NativeSdkVersion`. Removed in 2.0.0 per ADR-009. Consumers should migrate to `NativeSdkVersion`; legacy reads continue to work for one MINOR cycle.

## [1.3.0] - 2026-04-16

### Added
- ProGuard build-time smoke validator warns if `ReviewBridge` / callback interfaces not kept
- Trigger preset ScriptableObjects: CasualGame, HardcoreGame, UtilityApp (drop-in starting points)
- `WriteDiagnosticSnapshot(path)` file writer (development builds only, S2 security)
- `Documentation~/UPGRADE-1.x.md` consumer upgrade guide (1.0 → 1.3 path)
- `Documentation~/TELEMETRY-DASHBOARD.md` Firebase dashboard template with funnel + BigQuery SQL

## [1.2.0] - 2026-04-16

### Added
- `PreloadReviewInfo()` public API with 5-min TTL cache, auto-invalidates on app pause
- `IReviewAnalyticsAdapterV2` extended interface with 11 new methods (3 V1-context overloads + 8 new events)
- `ReviewTelemetryContext` structured analytics context (version, days, trigger reason, session, variant)
- `IFeedbackSink` interface + `NullFeedbackSink` default for negative-experience routing
- `ReviewThankYouToast` optional post-flow toast helper
- `FirebaseReviewAnalyticsAdapter` shipped sample (BIZSIM_FIREBASE guarded, 15 methods)
- `FileAppendFeedbackSink` shipped sample (writes to persistentDataPath)
- `BasicIntegrationBootstrap` press-Play scene bootstrap
- Per-version cooldown reset option (`ResetCooldownOnVersionChange`, default OFF)
- Editor Configuration "Telemetry" tab

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
