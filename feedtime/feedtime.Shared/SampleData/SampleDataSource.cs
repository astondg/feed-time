namespace FeedTime.SampleData
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using FeedTime.ViewModels;

    public sealed class SampleDataSource
    {
        public string CreateFeedText { get; set; }
        public string CreateSleepText { get; set; }
        public BabyViewModel CurrentBaby { get; set; }
        public DataTrendsViewModel DataTrends { get; set; }
        public MeasurementViewModel LatestMeasurement { get; set; }

        private ObservableCollection<FeedActivityViewModel> currentActivities = new ObservableCollection<FeedActivityViewModel>();
        public ObservableCollection<FeedActivityViewModel> CurrentActivities { get { return currentActivities; } }

        private ObservableCollection<FeedActivityViewModel> activityHistory = new ObservableCollection<FeedActivityViewModel>();
        public ObservableCollection<FeedActivityViewModel> ActivityHistory { get { return activityHistory; } }

        private ObservableCollection<BabyViewModel> babies = new ObservableCollection<BabyViewModel>();
        public ObservableCollection<BabyViewModel> Babies { get { return babies; } }
    }
}