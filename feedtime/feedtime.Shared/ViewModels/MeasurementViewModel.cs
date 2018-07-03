namespace FeedTime.ViewModels
{
    using System;
    using System.Windows.Input;
    using FeedTime.Common.Factories;
    using FeedTime.Common;
    using FeedTime.Common.DataModel;
    using FeedTime.Extensions;
    using Windows.Foundation;

    public class MeasurementViewModel : BaseViewModel
    {
        private bool serverActionIsRunning;
        private string id;
        private double? length;
        private double? weight;
        private DateTimeOffset? createdAt;
        private BabyViewModel baby;
        private RelayCommand create;
        private RelayCommand delete;

        public event TypedEventHandler<MeasurementViewModel, string> MeasurementCreated;

        public string Id
        {
            get { return id; }
            set { SetProperty(ref id, value); }
        }
        public double? Length
        {
            get { return length; }
            set { SetProperty(ref length, value); }
        }
        public double? Weight
        {
            get { return weight; }
            set { SetProperty(ref weight, value); }
        }
        public DateTimeOffset? CreatedAt
        {
            get { return createdAt; }
            set { SetProperty(ref createdAt, value); }
        }
        public BabyViewModel Baby
        {
            get { return baby; }
            set { SetProperty(ref baby, value); }
        }

        public ICommand Create
        {
            get
            {
                if (create == null)
                {
                    create = new RelayCommand(async () =>
                    {
                        Measurement measurement = null;

                        try
                        {
                            serverActionIsRunning = true;
                            create.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                            measurement = await DataSourceFactory.Current
                                                                 .CreateMeasurement(this.AsModel(SettingsViewModel.Current.UseMetricUnits));
                        }
                        finally
                        {
                            serverActionIsRunning = false;
                            create.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                        }

                        if (MeasurementCreated != null && measurement != null)
                            MeasurementCreated(this, measurement.Id);
                    }, () => !serverActionIsRunning);
                }

                return create;
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
                            serverActionIsRunning = true;
                            create.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                            await DataSourceFactory.Current
                                                   .DeleteMeasurement(this.AsModel(SettingsViewModel.Current.UseMetricUnits));
                        }
                        finally
                        {
                            serverActionIsRunning = false;
                            create.RaiseCanExecuteChanged();
                            delete.RaiseCanExecuteChanged();
                        }
                    }, () => !serverActionIsRunning && !string.IsNullOrWhiteSpace(Id));
                }

                return delete;
            }
        }
    }
}