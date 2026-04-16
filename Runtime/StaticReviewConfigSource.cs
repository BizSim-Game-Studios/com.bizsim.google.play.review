namespace BizSim.Google.Play.Review
{
    public sealed class StaticReviewConfigSource : IReviewConfigSource
    {
        public bool RemoteEnabled => true;
        public int? MinSessionCount => null;
        public int? MinDaysSinceInstall => null;
        public int? MinPromptIntervalDays => null;
        public int? MinLaunchCount => null;
        public string[] RequiredEvents => null;
        public string[] RequiredMilestones => null;
    }
}
