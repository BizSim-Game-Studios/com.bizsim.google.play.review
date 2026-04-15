# Basic Integration Sample

Two example scripts demonstrating both the callback-event and async/await styles for the Google Play In-App Review bridge.

## Scripts

- **`BasicReviewPrompt.cs`** — callback-event style. Subscribes to `ReviewController.Instance.OnReviewFlowCompleted` + `OnError` in `Start()`, calls `RequestReview()` on a button click.
- **`AsyncReviewPrompt.cs`** — async/await style. Calls `ReviewController.Instance.RequestReviewAsync(ct, 30f)` and awaits the result directly, with try/catch for error handling.

Pick ONE style for your project — don't wire both to the same button.

## First-run scene setup

**The sample does NOT ship a `.unity` scene file.** You'll need to create one in your host project on first use:

1. `File → New Scene → Basic (Built-in)` → save as `Assets/Samples/com.bizsim.google.play.review/1.0.0/BasicIntegration/BasicIntegration.unity` (Package Manager materializes the sample there when you click "Import" on the package's Samples list).
2. Add a **persistent GameObject** named `[ReviewController]`:
   - Attach the `ReviewController` component (from `Add Component → Scripts → BizSim.Google.Play.Review → ReviewController`).
   - In the Inspector, assign its `Mock Config` field to `MockPreset_Success_Fast` (run `Assets → Create → BizSim → Google Play → Review → Mock Presets` from the MockPresets sample first if you haven't already — it creates 6 preset assets in `Assets/BizSim/Review/MockPresets/`).
3. Add a **`Canvas`** (`GameObject → UI → Canvas`) with UI Scale Mode = "Scale With Screen Size".
4. Under the Canvas, add a **`Button`** named `RequestReviewButton` with text "Request Review".
5. Add an empty GameObject named `[SamplePrompt]` and attach `BasicReviewPrompt` (or `AsyncReviewPrompt`) to it.
6. Wire the Button's `OnClick()` event → `[SamplePrompt]` → `BasicReviewPrompt.OnRequestReviewButtonClicked` (or the async variant).
7. Save the scene. Press **Play**. The mock will fire `OnReviewFlowCompleted` after `SimulatedDelaySeconds` (0.1s for `MockPreset_Success_Fast`), and the log message appears in the Console.

## Integration steps for your own game

1. Install the package via Git URL (see root `README.md` Installation section for the OpenUPM scoped registry + Git URL two-step).
2. Run EDM4U Force Resolve (`Assets → External Dependency Manager → Android Resolver → Force Resolve`).
3. Drag `ReviewController` onto a persistent GameObject in your boot scene (it auto-`DontDestroyOnLoad`s itself).
4. Assign a `ReviewMockConfig` asset (from `Samples~/MockPresets` or create your own via `Assets → Create → BizSim → Google Play → Review → Mock Config`) to the controller's `Mock Config` field.
5. Wire a UI button's `OnClick` event to `BasicReviewPrompt.OnRequestReviewButtonClicked` (or `AsyncReviewPrompt.OnRequestReviewButtonClicked`).
6. Press **Play** in the Editor (uses mock) OR deploy to the Play Console internal test track (uses the real Play Core `:review:2.0.2` library).

## Quota-invisible semantics (important)

The Google Play In-App Review API is quota-invisible — `OnReviewFlowCompleted` firing does NOT mean the review dialog was shown. Do not gate rewards, prompts, or any user-visible behavior on "was the review shown?". See the main package README for details.
