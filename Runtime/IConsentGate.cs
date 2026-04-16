namespace BizSim.Google.Play.Review
{
    public interface IConsentGate
    {
        bool IsConsented(ReviewTriggerContext context);
    }
}
