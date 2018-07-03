namespace FeedTime.ViewModels
{
    using System;
    using System.Collections.ObjectModel;

    public class ActivityTrendViewModel : BaseViewModel
    {
        public DateTime Time { get; set; }
        public int IsActive { get; set; }
    }

    public class MoodTrendViewModel : BaseViewModel
    {
        public DateTime Date { get; set; }
        public int? Feeling { get; set; }
    }

    public class MeasurementTrendViewModel : BaseViewModel
    {
        public DateTime Date { get; set; }
        public int Week { get; set; }
        public double? Measurement { get; set; }
    }

    public class DataTrendsViewModel : BaseViewModel
    {
        public DateTimeOffset StartOfDay { get; set; }
        public DateTimeOffset StartOfWeek { get; set; }
        public DateTimeOffset BabysBirthDate { get; set; }

        public int AverageFeedsPerDay { get; set; }
        public TimeSpan AverageFeedDuration { get; set; }
        public int AverageFeedVolume { get; set; }
        public int AverageSleepsPerDay { get; set; }
        public TimeSpan AverageSleepDuration { get; set; }
        public int AverageChangesPerDay { get; set; }

        public ObservableCollection<ActivityTrendViewModel> FeedsOverLastDay { get; set; }
        public ObservableCollection<ActivityTrendViewModel> SleepsOverLastDay { get; set; }
        public ObservableCollection<ActivityTrendViewModel> ChangesOverLastDay { get; set; }
        public ObservableCollection<MoodTrendViewModel> BabysMoodOverLastWeek { get; set; }
        public ObservableCollection<MoodTrendViewModel> ParentsMoodOverLastWeek { get; set; }
        public ObservableCollection<MeasurementTrendViewModel> LengthSinceBirth { get; set; }
        public ObservableCollection<MeasurementTrendViewModel> WeightSinceBirth { get; set; }
    }
}