namespace FeedTime.ViewModels
{
    using System;
    using System.Windows.Input;
    using FeedTime.Common.Factories;
    using FeedTime.Common;
    using FeedTime.Common.DataModel;
    using FeedTime.Extensions;
    using Windows.Foundation;

    public class SleepActivityViewModel : ActivityViewModel
    {
        private RelayCommand update;
        private RelayCommand delete;

        public SleepActivityViewModel()
            : base("sleep") { }

        public event TypedEventHandler<SleepActivityViewModel, string> ActivityUpdated;

        public ICommand Update
        {
            get
            {
                if (update == null)
                {
                    update = new RelayCommand(async () =>
                    {
                        SleepActivity sleep = null;

                        try
                        {
                            ServerActionIsRunning = true;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                            if (!EndTime.HasValue && Duration > TimeSpan.Zero)
                                EndTime = StartDate.Add(Duration);

                            sleep = await DataSourceFactory.Current
                                                           .UpdateSleepActivity(this.AsModel());
                        }
                        finally
                        {
                            ServerActionIsRunning = false;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                        }

                        if (ActivityUpdated != null && sleep != null)
                            ActivityUpdated(this, sleep.Id);
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
                                                       .DeleteSleepActivity(this.AsModel());
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