namespace FeedTime.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using FeedTime.Common.DataModel;

    public class MainPageViewModel : BaseViewModel
    {
        private bool mainLoadingComplete;
        private bool canShowActivityByHourGraph;
        private bool canShowMoodByDayGraph;
        private bool canShowMeasurementsByWeekGraph;
        private bool showWeightMeasurement;
        private bool showLengthMeasurement;
        private int totalFeedsForDay;
        private int totalChangesForDay;
        private string createFeedText;
        private string createSleepText;
        private string currentWeightUnit;
        private string currentLengthUnit;
        private string nextActivity;
        private TimeSpan totalSleepTimeForDay;
        private DataTrendsViewModel dataTrends;
        private BabyViewModel currentBaby;
        private FeedActivityViewModel currentFeed;
        private SleepActivityViewModel currentSleep;
        private MeasurementViewModel latestMeasurement;

        public MainPageViewModel()
        {
            Babies = new ObservableCollection<BabyViewModel>();
            MostRecentActivities = new ObservableCollection<ActivityViewModel>();
            NextScheduledActivities = new ObservableCollection<Tuple<string, DateTimeOffset, bool>>();
        }

        public bool MainLoadingComplete
        {
            get { return mainLoadingComplete; }
            set { SetProperty(ref mainLoadingComplete, value); }
        }

        public bool CanShowActivityByHourGraph
        {
            get { return canShowActivityByHourGraph; }
            set { SetProperty(ref canShowActivityByHourGraph, value); }
        }

        public bool CanShowMoodByDayGraph
        {
            get { return canShowMoodByDayGraph; }
            set { SetProperty(ref canShowMoodByDayGraph, value); }
        }

        public bool CanShowMeasurementsByWeekGraph
        {
            get { return canShowMeasurementsByWeekGraph; }
            set { SetProperty(ref canShowMeasurementsByWeekGraph, value); }
        }

        public bool ShowWeightMeasurement
        {
            get { return showWeightMeasurement; }
            set { SetProperty(ref showWeightMeasurement, value); }
        }

        public bool ShowLengthMeasurement
        {
            get { return showLengthMeasurement; }
            set { SetProperty(ref showLengthMeasurement, value); }
        }

        public int TotalFeedsForDay
        {
            get { return totalFeedsForDay; }
            set { SetProperty(ref totalFeedsForDay, value); }
        }

        public int TotalChangesForDay
        {
            get { return totalChangesForDay; }
            set { SetProperty(ref totalChangesForDay, value); }
        }

        public string CreateFeedText
        {
            get { return createFeedText; }
            set { SetProperty(ref createFeedText, value); }
        }

        public string CreateSleepText
        {
            get { return createSleepText; }
            set { SetProperty(ref createSleepText, value); }
        }

        public string CurrentWeightUnit
        {
            get { return currentWeightUnit; }
            set { SetProperty(ref currentWeightUnit, value); }
        }

        public string CurrentLengthUnit
        {
            get { return currentLengthUnit; }
            set { SetProperty(ref currentLengthUnit, value); }
        }

        public string NextActivity
        {
            get { return nextActivity; }
            set { SetProperty(ref nextActivity, value); }
        }

        public TimeSpan TotalSleepTimeForDay
        {
            get { return totalSleepTimeForDay; }
            set { SetProperty(ref totalSleepTimeForDay, value); }
        }

        public DataTrendsViewModel DataTrends
        {
            get { return dataTrends; }
            set { SetProperty(ref dataTrends, value); }
        }

        public BabyViewModel CurrentBaby
        {
            get { return currentBaby; }
            set { SetProperty(ref currentBaby, value); }
        }

        public FeedActivityViewModel CurrentFeed
        {
            get { return currentFeed; }
            set { SetProperty(ref currentFeed, value); }
        }

        public SleepActivityViewModel CurrentSleep
        {
            get { return currentSleep; }
            set { SetProperty(ref currentSleep, value); }
        }

        public MeasurementViewModel LatestMeasurement
        {
            get { return latestMeasurement; }
            set { SetProperty(ref latestMeasurement, value); }
        }

        public ObservableCollection<BabyViewModel> Babies { get; set; }
        public ObservableCollection<ActivityViewModel> MostRecentActivities { get; set; }
        public ObservableCollection<Tuple<string, DateTimeOffset, bool>> NextScheduledActivities { get; set; }

        public void ResetFlags()
        {
            MainLoadingComplete = false;
            if (DataTrends == null)
            {
                CanShowActivityByHourGraph = false;
                CanShowMoodByDayGraph = false;
                CanShowMeasurementsByWeekGraph = false;
            }
        }
    }
}
