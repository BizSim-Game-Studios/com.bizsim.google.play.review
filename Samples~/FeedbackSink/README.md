# File Append Feedback Sink Sample

A minimal `IFeedbackSink` implementation that persists feedback entries as JSON lines.

## File location

Feedback is appended to:

```
Application.persistentDataPath/bizsim-feedback.jsonl
```

Each line is a self-contained JSON object:

```json
{"timestamp":"2026-04-16T12:00:00.0000000Z","text":"Great game but needs more levels"}
```

## Usage

```csharp
var sink = new FileAppendFeedbackSink();
ReviewController.Instance.SetFeedbackSink(sink);

// After your post-review feedback UI collects text:
ReviewController.Instance.SubmitFeedback(userText);
```

## Clearing feedback

The file grows monotonically. Consider clearing it:

- After uploading entries to your backend
- On app update (detect version change at boot)
- When storage exceeds a threshold

```csharp
var sink = new FileAppendFeedbackSink();
sink.Clear(); // deletes the file
```

## Custom file path

```csharp
var sink = new FileAppendFeedbackSink("/custom/path/feedback.jsonl");
```

## Important notes

- This sink is NOT wired to the Google Play review dialog. Google does not expose submitted review text to apps.
- The sink is a local-only capture mechanism for apps that show their own follow-up feedback UI.
- `SubmitFeedback` never throws (per `IFeedbackSink` contract). I/O failures are logged as warnings and swallowed.
