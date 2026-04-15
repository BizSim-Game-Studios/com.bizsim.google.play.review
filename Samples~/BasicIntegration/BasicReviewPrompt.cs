using UnityEngine;
using BizSim.Google.Play.Review;

public class BasicReviewPrompt : MonoBehaviour
{
    private void Start()
    {
        ReviewController.Instance.OnReviewFlowCompleted += OnCompleted;
        ReviewController.Instance.OnError += OnError;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid a dangling delegate on ReviewController (which persists across
        // scene loads via DontDestroyOnLoad). Sample code read as canonical by consumers.
        if (ReviewController.Instance == null) return;
        ReviewController.Instance.OnReviewFlowCompleted -= OnCompleted;
        ReviewController.Instance.OnError -= OnError;
    }

    public void OnRequestReviewButtonClicked()
    {
        if (!ReviewController.Instance.CanRequestReview())
        {
            Debug.Log($"[Sample] Local cooldown: {ReviewController.Instance.CooldownRemaining().TotalDays:F1} days remaining.");
            return;
        }
        ReviewController.Instance.RequestReview();
    }

    private void OnCompleted(ReviewResult r) =>
        Debug.Log($"[Sample] Review flow completed (source={r.Source}, elapsed={r.Elapsed.TotalMilliseconds:F0}ms).");

    private void OnError(ReviewError e) =>
        Debug.LogWarning($"[Sample] Review error: {e.Code} — {e.Message} (retryable={e.Retryable}).");
}
