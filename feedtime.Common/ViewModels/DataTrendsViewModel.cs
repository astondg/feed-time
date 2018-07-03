using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace FeedTime.ViewModels
{
    public class MoodTrend : BaseViewModel
    {
        public string Day { get;set; }
        public int Feeling { get; set; }
    }

    public class ActivityTrend : BaseViewModel
    {
        public string Hour { get; set; }
        public int IsActive { get; set; }
    }

    public class DataTrendsViewModel : BaseViewModel
    {
        public ObservableCollection<ActivityTrend> SleepsOverLastDay { get; set; }
        public ObservableCollection<ActivityTrend> FeedsOverLastDay { get; set; }
        public ObservableCollection<ActivityTrend> ChangesOverLastDay { get; set; }
        public ObservableCollection<MoodTrend> BabiesMoodOverLastWeek { get; set; }
        public ObservableCollection<MoodTrend> ParentsMoodOverLastWeek { get; set; }
    }
}
