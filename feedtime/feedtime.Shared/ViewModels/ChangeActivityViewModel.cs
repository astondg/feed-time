namespace FeedTime.ViewModels
{
    using System;
    using System.Windows.Input;
    using FeedTime.Common.Factories;
    using FeedTime.Common;
    using FeedTime.Common.DataModel;
    using FeedTime.Extensions;
    using Windows.Foundation;
    using Windows.ApplicationModel.Resources;

    public class ChangeActivityViewModel : ActivityViewModel
    {
        private int? nappiesUsed;
        private int? wipesUsed;
        private RelayCommand update;
        private RelayCommand delete;

        public ChangeActivityViewModel()
            : base("change") { }

        public event TypedEventHandler<ChangeActivityViewModel, string> ActivityUpdated;

        public int? NappiesUsed
        {
            get { return nappiesUsed; }
            set { SetProperty(ref nappiesUsed, value); }
        }
        public int? WipesUsed
        {
            get { return wipesUsed; }
            set { SetProperty(ref wipesUsed, value); }
        }

        public ICommand Update
        {
            get
            {
                if (update == null)
                {
                    update = new RelayCommand(async () =>
                    {
                        ChangeActivity change = null;
                        try
                        {
                            ServerActionIsRunning = true;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                            // A Change has no duration
                            EndTime = StartDate;

                            change = await DataSourceFactory.Current
                                                            .CreateChangeActivity(this.AsModel());
                        }
                        finally
                        {
                            ServerActionIsRunning = false;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                        }

                        if (ActivityUpdated != null && change != null)
                            ActivityUpdated(this, change.Id);
                    }, () => !ServerActionIsRunning);
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
                            await DataSourceFactory.Current
                                                   .DeleteChangeActivity(this.AsModel());
                        }
                        finally
                        {
                            ServerActionIsRunning = false;
                            update.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                        }
                    }, () => !ServerActionIsRunning && !string.IsNullOrWhiteSpace(Id));
                }

                return delete;
            }
        }
    }
}