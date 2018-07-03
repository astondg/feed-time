namespace FeedTime.Common.DataModel
{
    using System;
    using System.Collections.Generic;

    public sealed class ActivityTrend
    {
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset? EndTime { get; set; }
    }

    public sealed class MoodTrend
    {
        public DateTimeOffset Date { get; set; }
        public int Feeling { get; set; }
    }

    public sealed class MeasurementTrend
    {
        public DateTimeOffset Date { get; set; }
        public double? Length { get; set; }
        public double? Weight { get; set; }
    }

    public sealed class DataTrends
    {
        public string BabyId { get; set; }
        public DateTimeOffset TrendsGeneratedAt { get; set; }

        public DateTimeOffset StartOfDay { get; set; }
        public DateTimeOffset StartOfWeek { get; set; }
        public DateTimeOffset BabysBirthDate { get; set; }

        public int AverageFeedsPerDay { get; set; }
        public TimeSpan AverageFeedDuration { get; set; }
        public int AverageFeedVolume { get; set; }
        public int AverageSleepsPerDay { get; set; }
        public TimeSpan AverageSleepDuration { get; set; }
        public int AverageChangesPerDay { get; set; }

        public IEnumerable<ActivityTrend> SleepsOverLastDay { get; set; }
        public IEnumerable<ActivityTrend> FeedsOverLastDay { get; set; }
        public IEnumerable<ActivityTrend> ChangesOverLastDay { get; set; }

        public IEnumerable<MoodTrend> BabysMoodOverLastWeek { get; set; }
        public IEnumerable<MoodTrend> ParentsMoodOverLastWeek { get; set; }

        public IEnumerable<MeasurementTrend> MeasurementsSinceBirth { get; set; }
    }
}