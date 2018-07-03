namespace FeedTime.ViewModels
{
    using System;
    using System.Windows.Input;
    using FeedTime.Common;
    using FeedTime.Common.DataModel;
    using FeedTime.Common.Factories;
    using FeedTime.Extensions;
    using Windows.Foundation;

    public class FeedActivityViewModel : ActivityViewModel
    {
        private int? millilitresConsumed;
        private Side? feedingSide;
        private string currentVolumeUnit;
        private ICommand setFeedingSide;
        private RelayCommand update;
        private RelayCommand delete;

        public FeedActivityViewModel()
            : base("feed") { }

        public event TypedEventHandler<FeedActivityViewModel, string> ActivityUpdated;

        public int? MillilitresConsumed
        {
            get { return millilitresConsumed; }
            set { SetProperty(ref millilitresConsumed, value); }
        }

        public Side? FeedingSide
        {
            get { return feedingSide; }
            set { SetProperty(ref feedingSide, value); }
        }

        public string CurrentVolumeUnit
        {
            get { return currentVolumeUnit; }
            set { SetProperty(ref currentVolumeUnit, value); }
        }

        public ICommand SetFeedingSide
        {
            get
            {
                if (setFeedingSide == null)
                {
                    setFeedingSide = new RelayCommand<int>(param =>
                    {
                        FeedingSide = (Side)param;
                    });
                }

                return setFeedingSide;
            }
        }

        public ICommand Update
        {
            get
            {
                if (update == null)
                {
                    update = new RelayCommand(async () =>
                    {
                        FeedActivity feed = null;
                        try
                        {
                            ServerActionIsRunning = true;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                            if (!EndTime.HasValue && Duration > TimeSpan.Zero)
                                EndTime = StartDate.Add(Duration);

                            feed = await DataSourceFactory.Current
                                                          .UpdateFeedActivity(this.AsModel(SettingsViewModel.Current.UseMetricUnits));
                        }
                        finally
                        {
                            ServerActionIsRunning = false;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                        }

                        if (ActivityUpdated != null && feed != null)
                            ActivityUpdated(this, feed.Id);
                    }, () => !ServerActionIsRunning && !EndTime.HasValue);
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
                        try
                        {
                            ServerActionIsRunning = true;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                            if (!string.IsNullOrWhiteSpace(this.Id))
                                await DataSourceFactory.Current
                                                       .DeleteFeedActivity(this.AsModel(SettingsViewModel.Current.UseMetricUnits));
                        }
                        finally
                        {
                            ServerActionIsRunning = false;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                        }

                        if (ActivityUpdated != null)
                            ActivityUpdated(this, null);
                    }, () => !ServerActionIsRunning && !string.IsNullOrWhiteSpace(this.Id));
                }

                return delete;
            }
        }
    }
}