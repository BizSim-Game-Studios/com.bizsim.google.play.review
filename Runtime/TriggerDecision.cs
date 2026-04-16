using System;

namespace BizSim.Google.Play.Review
{
    public readonly struct TriggerDecision
    {
        public enum Kind { Allow, Block, Defer }

        public Kind Type { get; }
        public string Reason { get; }
        public TimeSpan MinDelay { get; }

        public bool IsAllow => Type == Kind.Allow;
        public bool IsBlock => Type == Kind.Block;
        public bool IsDefer => Type == Kind.Defer;

        TriggerDecision(Kind type, string reason, TimeSpan minDelay)
        {
            Type = type;
            Reason = reason;
            MinDelay = minDelay;
        }

        public static TriggerDecision Allow => new(Kind.Allow, null, TimeSpan.Zero);

        public static TriggerDecision Block(string reason) =>
            new(Kind.Block, reason ?? "unspecified", TimeSpan.Zero);

        public static TriggerDecision Defer(TimeSpan minDelay) =>
            new(Kind.Defer, null, minDelay);

        public override string ToString() => Type switch
        {
            Kind.Block => $"Block({Reason})",
            Kind.Defer => $"Defer({MinDelay.TotalSeconds:F0}s)",
            _ => "Allow"
        };
    }
}
