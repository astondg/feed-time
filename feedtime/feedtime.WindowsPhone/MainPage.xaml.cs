namespace FeedTime
{
    using System;
    using FeedTime.Common;
    using FeedTime.Common.Factories;
    using FeedTime.Extensions;
    using FeedTime.Pages;
    using FeedTime.Strings;
    using FeedTime.ViewModels;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : MainPageBase
    {
        private readonly StatusBar statusBar = StatusBar.GetForCurrentView();

        public MainPage()
            : base()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.NavigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.NavigationHelper.SaveState += this.NavigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            statusBar.ProgressIndicator.ProgressValue = 0;
            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_DEFAULT);
            await statusBar.ProgressIndicator.ShowAsync();

            DefaultViewModel.ResetFlags();
            var currentSettings = SettingsViewModel.Current;
            var currentDataSource = DataSourceFactory.Current;

            // This can happen when navigating back after creating a feed, sleep, change or measure
            // I.e. don't allow the user to return to the 'create' page for that item once it is created
            if (e.NavigationParameter != null
                    && string.Equals(Constants.NAVIGATION_PARAMETER_CLEARLASTBACKENTRY,
                                     e.NavigationParameter.ToString(),
                                     StringComparison.OrdinalIgnoreCase))
            {
                Frame.BackStack.RemoveAt(1);
            }

            // Set the default display units based on the user setting
            // Conversion of values between units is handled by the Model <-> ViewModel extensions
            DefaultViewModel.CurrentWeightUnit = currentSettings.UseMetricUnits
                                                        ? ResourceLoader.GetString(Constants.RESOURCEKEY_UNIT_WEIGHT_METRIC)
                                                        : ResourceLoader.GetString(Constants.RESOURCEKEY_UNIT_WEIGHT_IMPERIAL);
            DefaultViewModel.CurrentLengthUnit = currentSettings.UseMetricUnits
                                                        ? ResourceLoader.GetString(Constants.RESOURCEKEY_UNIT_LENGTH_METRIC)
                                                        : ResourceLoader.GetString(Constants.RESOURCEKEY_UNIT_LENGTH_IMPERIAL);

            await BackgroundTaskRegistrar.RegisterBackgroundTask(ResourceLoader.GetString(Constants.RESOURCEKEY_DIALOG_BACKGROUNDTASKSDISABLED));

            // Push any local changes, i.e. changes made while offline, to the server
            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_UPDATINGSERVER);
            await currentDataSource.PushLocalChangesToServer();

            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_BABY);
            var babyId = await RetrieveMostRecentlyAccessedBaby(currentSettings, currentDataSource);
            DefaultViewModel.Babies.Add(CurrentBaby);
            statusBar.ProgressIndicator.ProgressValue = 0.1;

            // Retrieve the activity schedule for the current baby,
            // may have already been downloaded by the background task
            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_SCHEDULE);
            var currentBabiesSchedule = (await currentDataSource.GetActivitySchedule(babyId)).AsViewModel(currentSettings.UseMetricUnits,
                                                                                                          CurrentBaby.AsModel());
            DefaultViewModel.NextActivity = GetNextActivityFromSchedule(CurrentBaby, currentBabiesSchedule);
            statusBar.ProgressIndicator.ProgressValue = 0.25;

            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_FEED);
            RetrieveCurrentFeed(currentSettings, currentBabiesSchedule);
            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_SLEEP);
            RetrieveCurrentSleep(currentSettings, currentBabiesSchedule);

            DefaultViewModel.MainLoadingComplete = true;
            statusBar.ProgressIndicator.ProgressValue = 0.5;

            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_BABIES);
            await RetrieveAllBabies(currentSettings, currentDataSource);
            statusBar.ProgressIndicator.ProgressValue = 0.6;

            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_MEASUREMENTS);
            await RetrieveLatestMeasurementsForCurrentBaby(currentSettings, currentDataSource, babyId);
            statusBar.ProgressIndicator.ProgressValue = 0.75;

            // Retrieve the status of the current baby
            if (currentBabiesSchedule.LastFeed != null)
                DefaultViewModel.MostRecentActivities.Add(currentBabiesSchedule.LastFeed);
            if (currentBabiesSchedule.LastSleep != null)
                DefaultViewModel.MostRecentActivities.Add(currentBabiesSchedule.LastSleep);
            if (currentBabiesSchedule.LastChange != null)
                DefaultViewModel.MostRecentActivities.Add(currentBabiesSchedule.LastChange);

            statusBar.ProgressIndicator.Text = ResourceLoader.GetString(Constants.RESOURCEKEY_LOADINGSTATUS_RHYTHM);
            await RetrieveDataTrendsForCurrentBaby(currentDataSource, babyId);
            statusBar.ProgressIndicator.ProgressValue = 1;

            statusBar.ProgressIndicator.Text = string.Empty;
            await statusBar.ProgressIndicator.HideAsync();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }
    }
}