namespace FeedTime.Pages
{
    using System;
    using System.Linq;
    using System.Threading;
    using FeedTime.Common;
    using FeedTime.Common.DataModel;
    using FeedTime.Common.Extensions;
    using FeedTime.Common.Factories;
    using FeedTime.Extensions;
    using FeedTime.Strings;
    using FeedTime.ViewModels;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.Core;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public class CreateFeedPageBase : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        private readonly FeedActivityViewModel viewModel;
        private readonly string activityName;
        private Timer durationUpdateTimer;

        public CreateFeedPageBase()
        {
            viewModel = new FeedActivityViewModel();
            viewModel.ActivityUpdated += viewModel_ActivityUpdated;
            this.navigationHelper = new NavigationHelper(this);
            activityName = resourceLoader.GetString(Constants.RESOURCEKEY_ACTIVITYNAME_FEED);
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        
        /// <summary>
        /// Gets the view model for this <see cref="Page"/>. This can be changed to a strongly typed view model.
        /// </summary>
        public FeedActivityViewModel DefaultViewModel
        {
            get { return viewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        protected async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var settings = SettingsViewModel.Current;
            DefaultViewModel.StartDate = DateTime.Now;
            DefaultViewModel.Baby = new BabyViewModel { Id = settings.MostRecentlyAccessedBabyId };

            var navigationParameter = e.NavigationParameter as ViewModelNavigationParameter<FeedActivityViewModel>;

            bool createNewFeed = string.IsNullOrWhiteSpace(DefaultViewModel.Id);

            if (navigationParameter != null)
            {
                if (Constants.NAVIGATION_PARAMETER_NFCINITIATED.Equals(navigationParameter.Flag))
                    navigationParameter.ViewModel.NfcInitiated = true;

                // If we have been passed an existing feed then display it
                if (navigationParameter.ViewModel != null)
                {
                    DefaultViewModel.From(navigationParameter.ViewModel);
                    createNewFeed = false;
                }
            }

            // Set up a timer to increment the duration every minute
            DisposeDurationUpdateTimer();
            if (!viewModel.EndTime.HasValue)
            {
                durationUpdateTimer = new Timer(async state =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            if (!viewModel.UserHasModifiedDuration)
                                viewModel.Duration = DateTimeOffset.Now - viewModel.StartDate;
                        });
                }, null, 60000, 60000);
            }

            if (createNewFeed)
            {
                // Start the feed and store it in Azure so it can be accessed on another device
                var createdActivity = await DataSourceFactory.Current
                                                             .CreateFeedActivity(viewModel.AsModel(settings.UseMetricUnits));
                DefaultViewModel.Id = createdActivity.Id;
                settings.InProgressFeedActivityId = createdActivity.Id;
                settings.LastActivityScheduleGeneratedAt = DateTimeOffset.MinValue;
                settings.LastDataTrendsGeneratedAt = DateTimeOffset.MinValue;

                // We've just created a new feed which invalidates any scheduled feed 
                // notifications so delete them
                if (!string.IsNullOrWhiteSpace(settings.NextFeedNotificationId))
                {
                    var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                    var scheduledNotificaiton = toastNotifier.GetScheduledToastNotifications()
                                                             .SingleOrDefault(tn => tn.Id == settings.NextFeedNotificationId);
                    if (scheduledNotificaiton != null)
                        toastNotifier.RemoveFromSchedule(scheduledNotificaiton);
                }

                var baby = await DataSourceFactory.Current.GetBaby(settings.MostRecentlyAccessedBabyId);
                var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
                tileUpdater.UpdateBabysActivityStatusTile(activityName,
                                                          new BabyActivityStatus
                                                            {
                                                                ActivityName = activityName,
                                                                BabyGivenName = baby.GivenName,
                                                                ActivityIsRunning = true,
                                                                LastActivityStartTime = DefaultViewModel.StartDate
                                                            });
            }

            if (!viewModel.EndTime.HasValue)
                viewModel.Duration = DateTimeOffset.Now - viewModel.StartDate;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        protected void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            DisposeDurationUpdateTimer();
        }

        protected void DurationPicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            viewModel.UserHasModifiedDuration = true;
        }

        private void DisposeDurationUpdateTimer()
        {
            if (durationUpdateTimer != null)
            {
                try
                {
                    durationUpdateTimer.Dispose();
                }
                finally
                {
                    durationUpdateTimer = null;
                }
            }
        }

        private void viewModel_ActivityUpdated(FeedActivityViewModel sender, string args)
        {
            var settings = SettingsViewModel.Current;

            if (sender.EndTime.HasValue || string.IsNullOrWhiteSpace(args))
                settings.InProgressFeedActivityId = null;

            settings.LastActivityScheduleGeneratedAt = DateTimeOffset.MinValue;
            settings.LastDataTrendsGeneratedAt = DateTimeOffset.MinValue;

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.UpdateBabysActivityStatusTile(activityName,
                                                      new BabyActivityStatus
                                                        {
                                                            ActivityName = activityName,
                                                            BabyGivenName = DefaultViewModel.Baby.GivenName,
                                                            ActivityIsRunning = false
                                                        });
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}