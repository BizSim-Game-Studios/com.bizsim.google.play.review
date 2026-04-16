# API Reference

Last reviewed: 2026-04-16

Namespace: `BizSim.Google.Play.Review`

## ReviewController

MonoBehaviour singleton. Entry point for all review operations.

| Member | Type | Description |
|--------|------|-------------|
| `Instance` | `ReviewController` | Lazy singleton; creates a DontDestroyOnLoad GameObject on first access |
| `RequestReviewAsync(ct, timeout)` | `Task<ReviewResult>` | Requests and launches the native review flow |
| `RequestReview()` | `void` | Fire-and-forget review request with event callbacks |
| `OnReviewCompleted` | `event Action<ReviewResult>` | Fired when the review flow completes |
| `OnReviewError` | `event Action<ReviewError>` | Fired on error |
| `ClearLocalCooldownForTesting()` | `void` | Deletes the PlayerPrefs cooldown key (QA only) |
| `SetAnalyticsAdapter(adapter)` | `void` | Injects an analytics adapter or null to disable |
| `SetConsentGate(gate)` | `void` | Injects a GDPR consent gate |
| `SetFeedbackSink(sink)` | `void` | Injects a feedback sink |
| `GetDiagnosticSnapshot()` | `ReviewDiagnosticSnapshot` | Returns current controller state for debugging |

## IReviewProvider

DI interface implemented by `AndroidReviewProvider` and `MockReviewProvider`.

| Method | Description |
|--------|-------------|
| `RequestReviewAsync(ct, timeout)` | Requests and launches the review flow |
| `Dispose()` | Cleans up native resources |

## ReviewSettings

ScriptableObject at `Assets/Resources/BizSim/GooglePlay/ReviewSettings.asset`.

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `LogsEnabled` | `bool` | `true` | Master log switch |
| `LogLevel` | `LogLevel` | `Info` | Minimum log severity |
| `UseMockInDevelopmentBuild` | `bool` | `false` | Use mock provider in Development Builds |
| `EnableAnalyticsByDefault` | `bool` | `false` | Auto-enable analytics adapter |
| `CooldownDays` | `int` | `90` | Days between review requests |
| `MinSessionsBeforePrompt` | `int` | `5` | Minimum app sessions before first prompt |
| `MinDaysAfterInstall` | `int` | `7` | Minimum days since install before first prompt |

## ReviewResult

Readonly struct returned by `RequestReviewAsync`.

| Property | Type | Description |
|----------|------|-------------|
| `FlowCompleted` | `bool` | Whether the API call completed without error |
| `Source` | `ReviewSource` | `Android` or `Mock` |
| `ElapsedMs` | `long` | Wall-clock milliseconds for the flow |

## ReviewData (enums)

| Enum | Values |
|------|--------|
| `ReviewErrorCode` | `NoError`, `PlayStoreNotFound`, `InvalidRequest`, `InternalError`, `ApiNotAvailable` |
| `ReviewSource` | `Android`, `Mock` |

## IReviewAnalyticsAdapterV2

Telemetry contract. Implement to log review events to your analytics backend.

| Method | Description |
|--------|-------------|
| `OnReviewRequested()` | Called when a review request begins |
| `OnFlowCompleted(source, elapsedMs)` | Called on successful completion |
| `OnError(code, retryable)` | Called on error |
| `OnCooldownCleared()` | Called when cooldown is reset |
| `OnTriggerEvaluated(decision)` | Called after trigger engine evaluation |

## IConsentGate

GDPR consent interface.

| Method | Description |
|--------|-------------|
| `IsConsentGrantedAsync()` | Returns `Task<bool>`; review is skipped if false |

## ReviewMockConfig

ScriptableObject for editor and non-Android testing. Configures simulated behavior.

| Field | Type | Description |
|-------|------|-------------|
| `SimulatedResult` | `MockReviewResult` | Success, Error, or Timeout |
| `SimulatedErrorCode` | `ReviewErrorCode` | Error code to simulate |
| `SimulatedDelayMs` | `int` | Artificial delay in milliseconds |
