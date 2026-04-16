# Trigger Presets

Pre-configured `ReviewSettings` assets for three common app categories.

## Usage

1. Import the sample via Unity Package Manager.
2. Run **BizSim > Google Play > Review > Create Trigger Presets**.
3. Three assets are created under `Assets/BizSim/Review/TriggerPresets/`.
4. Drag the desired preset into the `Assets/Resources/BizSim/GooglePlay/` folder
   (rename it to `ReviewSettings.asset` to make it the active project-wide config),
   or use it as a reference when configuring your own settings via the Configuration window.

## Presets

| Preset | Grace Sessions | Grace Days | Use Case |
|---|---|---|---|
| `CasualGameTrigger` | 5 | 3 | High early engagement; prompt while active |
| `HardcoreGameTrigger` | 10 | 14 | Longer evaluation period before prompting |
| `UtilityAppTrigger` | 3 | 7 | Short sessions, infrequent use; prompt early |
