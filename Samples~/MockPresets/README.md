# Mock Presets Sample

Editor-only menu action that materializes 6 `ReviewMockConfig` asset scenarios into your host project.

## Usage

1. Import this sample from the Package Manager (`Window → Package Manager → BizSim Google Play In-App Review Bridge → Samples → Mock Config Presets → Import`).
2. Run **`Assets → Create → BizSim → Google Play → Review → Mock Presets`** from the top menu.
3. Six preset assets appear in `Assets/BizSim/Review/MockPresets/`.
4. Drag any preset onto `ReviewController._mockConfig` in the Inspector to switch scenarios.

## Preset list

| Name | SimulatedDelaySeconds | SimulatedError | SimulateFlowShown | Notes |
|---|---|---|---|---|
| `MockPreset_Success_Fast` | 0.1 | NoError | true | Quick success, matches typical device behavior |
| `MockPreset_Success_SlowNetwork` | 3.0 | NoError | true | Tests timeout guard + UI spinner |
| `MockPreset_PlayStoreNotFound` | 0.1 | PlayStoreNotFound | false | Device without Google Play |
| `MockPreset_InvalidRequest` | 0.1 | InvalidRequest | false | Malformed state |
| `MockPreset_InternalError` | 0.5 | InternalError | false | Transient — caller should retry |
| `MockPreset_Success_DialogNotShown` | 0.2 | NoError | false | Exercises "Task succeeded but dialog was not shown" path — most realistic production shape because Google's API hides this from us |

## Why a script instead of shipped `.asset` files?

`ScriptableObject` asset GUIDs are project-specific and cause diff noise when shipped in a package's `Samples~` folder. A menu-action generator lets each consumer materialize fresh presets in their own project without GUID collisions. The resulting `.asset` files live under `Assets/BizSim/Review/MockPresets/` and are committed with the consumer's game project, not this package.
