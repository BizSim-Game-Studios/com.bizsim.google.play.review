namespace BizSim.Google.Play.Review
{
    public interface IReviewConfigSource
    {
        bool RemoteEnabled { get; }
        int? MinSessionCount { get; }
        int? MinDaysSinceInstall { get; }
        int? MinPromptIntervalDays { get; }
        int? MinLaunchCount { get; }
        string[] RequiredEvents { get; }
        string[] RequiredMilestones { get; }
    }
}
