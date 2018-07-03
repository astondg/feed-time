namespace FeedTime.ViewModels
{
    using System;

    public class ActivityScheduleViewModel : BaseViewModel
    {
        public string BabyId { get; set; }
        public DateTimeOffset ScheduleGeneratedAt { get; set; }

        public bool CurrentlyFeeding { get; set; }
        public FeedActivityViewModel LastFeed { get; set; }
        public TimeSpan? AverageTimeBetweenFeeds { get; set; }
        public DateTimeOffset NextFeedDueAt { get; set; }

        public bool CurrentlySleeping { get; set; }
        public SleepActivityViewModel LastSleep { get; set; }
        public TimeSpan? AverageTimeBetweenSleeps { get; set; }
        public DateTimeOffset NextSleepDueAt { get; set; }

        public ChangeActivityViewModel LastChange { get; set; }
        public TimeSpan? AverageTimeBetweenChanges { get; set; }
        public DateTimeOffset NextChangeDueAt { get; set; }
    }
}