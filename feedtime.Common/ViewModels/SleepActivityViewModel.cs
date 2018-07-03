namespace FeedTime.ViewModels
{
    using System.Windows.Input;
    using FeedTime.Common;
    using FeedTime.DataModel;
    using FeedTime.Extensions;

    public class SleepActivityViewModel : ActivityViewModel
    {
        private ICommand createSleep;
        private ICommand deleteSleep;

        public SleepActivityViewModel()
            : base("sleep") { }

        public ICommand Create
        {
            get
            {
                if (createSleep == null)
                {
                    createSleep = new RelayCommand(async () =>
                    {
                        if (!EndTime.HasValue && Duration > 0)
                            EndTime = StartTime.AddMinutes(Duration);

                        var feed = await MobileServicesDataSource.Current
                                                                 .CreateActivity(this.AsModel());
                        Id = feed.Id;
                    });
                }

                return createSleep;
            }
        }

        public ICommand Delete
        {
            get
            {
                if (deleteSleep == null)
                {
                    deleteSleep = new RelayCommand(async () =>
                    {
                        await MobileServicesDataSource.Current
                                                      .DeleteActivity(this.AsModel());
                    });
                }

                return deleteSleep;
            }
        }
    }
}