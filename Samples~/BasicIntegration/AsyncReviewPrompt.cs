using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BizSim.Google.Play.Review;

public class AsyncReviewPrompt : MonoBehaviour
{
    public async void OnRequestReviewButtonClicked()
    {
        try
        {
            var result = await ReviewController.Instance.RequestReviewAsync(CancellationToken.None, 30f);
            Debug.Log($"[Sample] Completed: source={result.Source}, elapsed={result.Elapsed.TotalMilliseconds:F0}ms");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[Sample] Async review flow failed: {ex.Message}");
        }
    }
}
