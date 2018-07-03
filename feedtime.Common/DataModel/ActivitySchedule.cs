namespace FeedTime.DataModel
{
    using System;

    public class ActivitySchedule
    {
        public string BabyId { get; set; }
        public DateTimeOffset ScheduleGeneratedAt { get; set; }

        public bool CurrentlyFeeding { get; set; }
        public DateTimeOffset? LastFeedTime { get; set; }
        public TimeSpan? AverageTimeBetweenFeeds { get; set; }

        public bool CurrentlySleeping { get; set; }
        public DateTimeOffset? LastSleepTime { get; set; }
        public TimeSpan? AverageTimeBetweenSleeps { get; set; }

        public DateTimeOffset? LastChangeTime { get; set; }
        public TimeSpan? AverageTimeBetweenChanges { get; set; }
    }
}