namespace FeedTime.Common.DataModel
{
    using System;

    public sealed class ActivitySchedule
    {
        public string BabyId { get; set; }
        public DateTimeOffset ScheduleGeneratedAt { get; set; }

        public bool CurrentlyFeeding { get; set; }
        public FeedActivity LastFeed { get; set; }
        public TimeSpan? AverageTimeBetweenFeeds { get; set; }
        public DateTimeOffset NextFeedDueAt { get; set; }

        public bool CurrentlySleeping { get; set; }
        public SleepActivity LastSleep { get; set; }
        public TimeSpan? AverageTimeBetweenSleeps { get; set; }
        public DateTimeOffset NextSleepDueAt { get; set; }

        public ChangeActivity LastChange { get; set; }
        public TimeSpan? AverageTimeBetweenChanges { get; set; }
        public DateTimeOffset NextChangeDueAt { get; set; }
    }
}