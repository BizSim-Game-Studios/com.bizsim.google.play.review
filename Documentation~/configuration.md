# Configuration

Last reviewed: 2026-04-16

## ReviewSettings asset

The project-wide defaults are stored in a ScriptableObject at:

```
Assets/Resources/BizSim/GooglePlay/ReviewSettings.asset
```

This asset is auto-created by `ReviewSettingsAsset.LoadOrCreate()` the first time you open
the Configuration window. The controller reads it at `Awake()` via `Resources.Load`.

### Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `LogsEnabled` | `bool` | `true` | Master switch for all `[BizSim.Review]` log output |
| `LogLevel` | `LogLevel` | `Info` | Minimum severity: Verbose, Info, Warning, Error, Silent |
| `UseMockInDevelopmentBuild` | `bool` | `false` | When true, Development Builds use the mock provider instead of the real JNI bridge |
| `EnableAnalyticsByDefault` | `bool` | `false` | Auto-registers the default analytics adapter at startup |
| `CooldownDays` | `int` | `90` | Minimum days between review requests |
| `MinSessionsBeforePrompt` | `int` | `5` | App must be launched at least this many times before the first review prompt |
| `MinDaysAfterInstall` | `int` | `7` | Minimum elapsed days since install before the first review prompt |
| `EnableKillSwitch` | `bool` | `false` | When true, all review requests are silently suppressed |

### Per-instance overrides

`ReviewController` has matching `[SerializeField]` fields. When a MonoBehaviour field has
a non-default value, it overrides the asset value for that instance. This lets you have
different settings per scene or per prefab variant.

## Editor Configuration window

Open via **BizSim > Google Play > Review > Configuration**.

### Sections

1. **Package Info** — displays current package version, Play Core version, and EDM4U status.
2. **Settings** — draws the `ReviewSettings` asset with full `SerializedObject` editing.
   - **Apply** — saves changes to disk and calls `BizSimLogger.InvalidateCache()`.
   - **Revert** — discards unsaved changes.
   - **Reset to defaults** — restores all fields to their default values.
3. **Diagnostics** — shows the current controller state (cooldown expiry, session count,
   last prompt timestamp).
4. **Quick Actions** — buttons for Force Resolve, Clear Cooldown, and Open Samples.

### Log level changes

After clicking Apply, log level changes take effect immediately without a domain reload.
The Configuration window calls `BizSimLogger.InvalidateCache()` which clears the cached
settings reference inside `BizSimLogger`.
