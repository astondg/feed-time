namespace FeedTime.ViewModels
{
    using System.Windows.Input;
    using FeedTime.Common;
    using FeedTime.DataModel;
    using FeedTime.Extensions;

    public class FeedActivityViewModel : ActivityViewModel
    {
        private int? millilitresConsumed;
        private ICommand update;
        private ICommand delete;

        public FeedActivityViewModel()
            : base("feed") { }

        public int? MillilitresConsumed
        {
            get { return millilitresConsumed; }
            set { SetProperty(ref millilitresConsumed, value); }
        }

        public ICommand Update
        {
            get
            {
                if (update == null)
                {
                    update = new RelayCommand(async () =>
                    {
                        if (!EndTime.HasValue && Duration > 0)
                            EndTime = StartTime.AddMinutes(Duration);

                        var feed = await MobileServicesDataSource.Current
                                                                 .UpdateActivity(this.AsModel());
                        Id = feed.Id;
                    });
                }

                return update;
            }
        }

        public ICommand Delete
        {
            get
            {
                if (delete == null)
                {
                    delete = new RelayCommand(async () =>
                    {
                        await MobileServicesDataSource.Current
                                                      .DeleteActivity(this.AsModel());
                    });
                }

                return delete;
            }
        }
    }
}