namespace BizSim.Google.Play.Review
{
    public interface IReviewTriggerEngine
    {
        TriggerDecision Evaluate(ReviewTriggerContext context);
    }
}
