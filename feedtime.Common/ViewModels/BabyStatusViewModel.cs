namespace FeedTime.ViewModels
{
    using System.Collections.ObjectModel;

    public class BabyStatusViewModel : BaseViewModel
    {
        private double? averageMinutesSleepPerDayLastWeek;
        private double? averageMinutesFeedPerDayLastWeek;
        private double? averageSleepDurationLastWeek;
        private double? averageFeedDurationLastWeek;
        private double? averageFeedVolumeLastWeek;

        public ObservableCollection<ActivityViewModel> MostRecentActivities { get; set; }
    }
}