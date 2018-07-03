namespace FeedTime.Common.DataModel
{
    using System;

    public sealed class BabyActivityStatus
    {
        public string ActivityName { get; set; }
        public string BabyGivenName { get; set; }
        public bool ActivityIsRunning { get; set; }
        public DateTimeOffset? LastActivityStartTime { get; set; }
        public DateTimeOffset? NextActivityStartTime { get; set; }
    }
}