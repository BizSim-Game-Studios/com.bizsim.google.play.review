using System.Collections;
using UnityEngine;
using BizSim.Google.Play.Review;

/// <summary>
/// Self-contained bootstrap for the BasicIntegration sample scene. On Start:
/// records an event and a session, waits 2 seconds, then evaluates the trigger
/// engine and (if allowed) requests the review flow via the mock provider.
/// <para>
/// <b>Policy note (P3):</b> This sample uses <c>RecordEvent</c>, NOT a user-tappable
/// button, to trigger the review flow. Per Google Play policy, never wire
/// <c>RequestReview</c> to a button that the user can tap directly.
/// </para>
/// </summary>
public class BasicIntegrationBootstrap : MonoBehaviour
{
    [Tooltip("Optional mock config for editor testing. Assign a MockPreset asset here.")]
    [SerializeField] private ReviewMockConfig _mockConfig;

    private IEnumerator Start()
    {
        var controller = ReviewController.Instance;
        if (controller == null)
        {
            Debug.LogError("[BasicIntegration] ReviewController.Instance is null. Cannot run sample.");
            yield break;
        }

        // Subscribe to events
        controller.OnReviewFlowCompleted += OnCompleted;
        controller.OnError += OnError;

        // Record telemetry signals
        controller.RecordEvent("sample_started");
        controller.RecordSession();
        Debug.Log("[BasicIntegration] Recorded event 'sample_started' and a session. Waiting 2 seconds...");

        // Simulate a short delay (e.g., player completes a level)
        yield return new WaitForSeconds(2f);

        // Check if the trigger engine allows a review prompt
        if (!controller.CanRequestReview())
        {
            Debug.Log($"[BasicIntegration] Cooldown active — {controller.CooldownRemaining().TotalDays:F1} days remaining. Skipping review request.");
            yield break;
        }

        // Request the review flow. In Editor this uses the mock provider.
        // On device with the real provider, the Google Play review dialog may or may not appear
        // (quota-invisible semantics — see README).
        Debug.Log("[BasicIntegration] Requesting review flow...");
        controller.RequestReview("sample_basic_integration");
    }

    private void OnDestroy()
    {
        var controller = ReviewController.Instance;
        if (controller == null) return;
        controller.OnReviewFlowCompleted -= OnCompleted;
        controller.OnError -= OnError;
    }

    private void OnCompleted(ReviewResult r)
    {
        Debug.Log($"[BasicIntegration] Review flow completed (source={r.Source}, elapsed={r.Elapsed.TotalMilliseconds:F0}ms).");
    }

    private void OnError(ReviewError e)
    {
        Debug.LogWarning($"[BasicIntegration] Review error: {e.Code} — {e.Message} (retryable={e.Retryable}).");
    }
}
