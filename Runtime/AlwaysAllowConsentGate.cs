namespace BizSim.Google.Play.Review
{
    public sealed class AlwaysAllowConsentGate : IConsentGate
    {
        public bool IsConsented(ReviewTriggerContext context) => true;
    }
}
