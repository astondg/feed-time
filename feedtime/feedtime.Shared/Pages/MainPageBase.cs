namespace FeedTime.Pages
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FeedTime.Common;
    using FeedTime.Extensions;
    using FeedTime.Strings;
    using FeedTime.ViewModels;
    using Windows.ApplicationModel.Resources;
#if WINDOWS_PHONE_APP
    using Windows.Media.SpeechRecognition;
#endif
    using Windows.Storage;
    using Windows.UI.ViewManagement;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Input;
    using Windows.UI.Xaml.Navigation;
#if !WINDOWS_PHONE_APP
    using Windows.UI.ApplicationSettings;
#endif

    public class MainPageBase : Page
    {        
        private readonly NavigationHelper navigationHelper;
        private readonly MainPageViewModel defaultViewModel;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        private readonly string activityNameFeed, activityNameSleep;

        public MainPageBase()
        {
            defaultViewModel = new MainPageViewModel();
            CurrentBaby = null;
            activityNameFeed = resourceLoader.GetString(Constants.RESOURCEKEY_ACTIVITYNAME_FEED);
            activityNameSleep = resourceLoader.GetString(Constants.RESOURCEKEY_ACTIVITYNAME_SLEEP);

            this.navigationHelper = new NavigationHelper(this);
        }

        /// <summary>
        /// The resource loader for the current view
        /// </summary>
        protected ResourceLoader ResourceLoader { get { return resourceLoader; } }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public MainPageViewModel DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        protected BabyViewModel CurrentBaby { get; set; }

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
        protected virtual void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        { }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        protected virtual void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        { }

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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
#if WINDOWS_PHONE_APP
            // Install voice commands, safe to run on each new navigation
            if (e.NavigationMode == NavigationMode.New)
            {
                var voiceCommandFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceCommandDefinition_8.1.xml"));
                await VoiceCommandManager.InstallCommandSetsFromStorageFileAsync(voiceCommandFile);
            }
#endif

            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        #region Loading Methods
        protected async Task RetrieveDataTrendsForCurrentBaby(Common.DataModel.MobileServicesDataSource currentDataSource, string babyId)
        {
            var latestDataTrendsRaw = await currentDataSource.GetDataTrends(babyId);
            var latestDataTrends = latestDataTrendsRaw.AsViewModel(CurrentBaby.DateOfBirth);
            DefaultViewModel.DataTrends = latestDataTrends;
            DefaultViewModel.CanShowActivityByHourGraph = latestDataTrends.FeedsOverLastDay != null && latestDataTrends.FeedsOverLastDay.Any()
                                                                || latestDataTrends.SleepsOverLastDay != null && latestDataTrends.SleepsOverLastDay.Any()
                                                                || latestDataTrends.ChangesOverLastDay != null && latestDataTrends.ChangesOverLastDay.Any();
            DefaultViewModel.CanShowMoodByDayGraph = latestDataTrends.BabysMoodOverLastWeek != null && latestDataTrends.BabysMoodOverLastWeek.Any()
                                                        || latestDataTrends.ParentsMoodOverLastWeek != null && latestDataTrends.ParentsMoodOverLastWeek.Any();
            DefaultViewModel.CanShowMeasurementsByWeekGraph = (latestDataTrends.LengthSinceBirth != null && latestDataTrends.LengthSinceBirth.Any())
                                                                    || (latestDataTrends.WeightSinceBirth != null && latestDataTrends.WeightSinceBirth.Any());
            var startOfCurrentDay = DateTimeOffset.Now.Date;
            DefaultViewModel.TotalFeedsForDay = latestDataTrendsRaw.FeedsOverLastDay
                                                                   .Count(feed => feed.StartTime.ToLocalTime() >= startOfCurrentDay);
            DefaultViewModel.TotalSleepTimeForDay = latestDataTrendsRaw.SleepsOverLastDay
                                                                       .Where(sleep => sleep.StartTime.ToLocalTime() >= startOfCurrentDay)
                                                                       .Aggregate(TimeSpan.Zero,
                                                                                  (a,b) => a.Add((b.EndTime ?? DateTime.UtcNow) - b.StartTime),
                                                                                  a => a);
            DefaultViewModel.TotalChangesForDay = latestDataTrendsRaw.ChangesOverLastDay
                                                                     .Count(change => change.StartTime.ToLocalTime() >= startOfCurrentDay);
        }

        protected async Task RetrieveLatestMeasurementsForCurrentBaby(SettingsViewModel currentSettings, Common.DataModel.MobileServicesDataSource currentDataSource, string babyId)
        {
            var latestMeasurement = await currentDataSource.GetLatestMeasurement(babyId);
            DefaultViewModel.LatestMeasurement = latestMeasurement != null ? latestMeasurement.AsViewModel(currentSettings.UseMetricUnits, CurrentBaby.AsModel()) : null;
            DefaultViewModel.ShowWeightMeasurement = latestMeasurement != null && latestMeasurement.Weight.HasValue;
            DefaultViewModel.ShowLengthMeasurement = latestMeasurement != null && latestMeasurement.Length.HasValue;
        }

        protected async Task RetrieveAllBabies(SettingsViewModel currentSettings, Common.DataModel.MobileServicesDataSource currentDataSource)
        {
            var babies = await currentDataSource.GetBabies(currentSettings.FamilyId);
            foreach (var baby in babies)
            {
                // Current baby was already added when loaded
                if (baby.Id == CurrentBaby.Id)
                    continue;

                DefaultViewModel.Babies.Add(baby.AsViewModel());
            }
        }

        protected void RetrieveCurrentSleep(SettingsViewModel currentSettings, ActivityScheduleViewModel currentBabiesSchedule)
        {
            if (currentBabiesSchedule.CurrentlySleeping)
            {
                currentSettings.InProgressSleepActivityId = currentBabiesSchedule.LastSleep.Id;
                DefaultViewModel.CurrentSleep = currentBabiesSchedule.LastSleep;
                var activityDuration = (DateTime.Now - currentBabiesSchedule.LastSleep.StartDate);
                string currentSleepStatus = activityDuration.Hours > 0
                                                ? string.Format(resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYBUTTONHOURS),
                                                                            activityNameSleep,
                                                                            Math.Floor(activityDuration.TotalHours),
                                                                            activityDuration.Minutes)
                                                : string.Format(resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYBUTTONMINUTES),
                                                                activityNameSleep,
                                                                Math.Floor(activityDuration.TotalMinutes));
                DefaultViewModel.CreateSleepText = currentBabiesSchedule.LastSleep.EndTime == null
                                                        ? currentSleepStatus
                                                        : activityNameSleep;
            }
            else
            {
                DefaultViewModel.CurrentSleep = null;
                DefaultViewModel.CreateSleepText = activityNameSleep;
            }
        }

        protected void RetrieveCurrentFeed(SettingsViewModel currentSettings, ActivityScheduleViewModel currentBabiesSchedule)
        {
            if (currentBabiesSchedule.CurrentlyFeeding)
            {
                currentSettings.InProgressFeedActivityId = currentBabiesSchedule.LastFeed.Id;
                DefaultViewModel.CurrentFeed = currentBabiesSchedule.LastFeed;
                var activityDuration = (DateTime.Now - currentBabiesSchedule.LastFeed.StartDate);
                string currentFeedStatus = activityDuration.Hours > 0
                                            ? string.Format(resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYBUTTONHOURS),
                                                                        activityNameFeed,
                                                                        Math.Floor(activityDuration.TotalHours),
                                                                        activityDuration.Minutes)
                                            : string.Format(resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYBUTTONMINUTES),
                                                            activityNameFeed,
                                                            Math.Floor(activityDuration.TotalMinutes));
                DefaultViewModel.CreateFeedText = currentBabiesSchedule.LastFeed.EndTime == null
                                                        ? currentFeedStatus
                                                        : activityNameFeed;
            }
            else
            {
                DefaultViewModel.CurrentFeed = null;
                DefaultViewModel.CreateFeedText = activityNameFeed;
            }
        }

        protected async Task<string> RetrieveMostRecentlyAccessedBaby(SettingsViewModel currentSettings, Common.DataModel.MobileServicesDataSource currentDataSource)
        {
            var babyId = currentSettings.MostRecentlyAccessedBabyId;
            if (CurrentBaby == null)
            {
                var baby = await currentDataSource.GetBaby(babyId);
                CurrentBaby = baby.AsViewModel();
            }
            CurrentBaby.ShowAgeInWeeks = SettingsViewModel.Current.ShowAgeInWeeks;
            DefaultViewModel.CurrentBaby = CurrentBaby;
            DefaultViewModel.PropertyChanged += DefaultViewModel_PropertyChanged;
            return babyId;
        }

        protected string GetNextActivityFromSchedule(BabyViewModel currentBaby, ActivityScheduleViewModel currentBabiesSchedule)
        {
            if (currentBabiesSchedule.LastFeed == null
                && currentBabiesSchedule.LastSleep == null
                && currentBabiesSchedule.LastChange == null)
                return resourceLoader.GetString(Constants.RESOURCEKEY_BLURB_WELCOMETOFEEDTIME);

            DefaultViewModel.NextScheduledActivities.Add(Tuple.Create(activityNameFeed, currentBabiesSchedule.NextFeedDueAt, currentBabiesSchedule.CurrentlyFeeding));
            DefaultViewModel.NextScheduledActivities.Add(Tuple.Create(activityNameSleep, currentBabiesSchedule.NextSleepDueAt, currentBabiesSchedule.CurrentlySleeping));
            DefaultViewModel.NextScheduledActivities.Add(Tuple.Create(resourceLoader.GetString(Constants.RESOURCEKEY_ACTIVITYNAME_CHANGE), currentBabiesSchedule.NextChangeDueAt, false));

            var nextActivity = DefaultViewModel.NextScheduledActivities
                                                .Where(i => i.Item2 != null && !i.Item3)
                                                .MinBy(i => i.Item2);

            var nextActivityDueIn = nextActivity.Item2 - DateTime.Now;
            var nextActivityBuilder = new StringBuilder(currentBaby.GivenName);
            nextActivityBuilder.AppendFormat(resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYDUETO),
                                             nextActivity.Item1);
            if (nextActivityDueIn > TimeSpan.Zero)
            {
                string dueInFormat = nextActivityDueIn.Hours > 0
                                        ? resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYINHOURS)
                                        : resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYINMINUTES);
                nextActivityBuilder.AppendFormat(CultureInfo.CurrentCulture, dueInFormat, nextActivityDueIn.Duration());
            }
            else
            {
                string dueAgoFormat = nextActivityDueIn.Hours > 0
                                        ? resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYAGOHOURS)
                                        : resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYAGOMINUTES);
                nextActivityBuilder.AppendFormat(CultureInfo.CurrentCulture, dueAgoFormat, nextActivityDueIn.Duration());
            }

            return nextActivityBuilder.ToString();
        }
        #endregion

        #region Event Handlers
        protected void FeedButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModelNavigationParameter<FeedActivityViewModel> navigationParameter = null;
            navigationParameter = DefaultViewModel.CurrentFeed != null
                                    ? new ViewModelNavigationParameter<FeedActivityViewModel>(DefaultViewModel.CurrentFeed)
                                    : null;
            if (!Frame.Navigate(typeof(CreateFeedPage), navigationParameter))
            {
                throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
            }
        }

        protected void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModelNavigationParameter<SleepActivityViewModel> navigationParameter = null;
            navigationParameter = DefaultViewModel.CurrentSleep != null
                                    ? new ViewModelNavigationParameter<SleepActivityViewModel>(DefaultViewModel.CurrentSleep)
                                    : null;
            if (!Frame.Navigate(typeof(CreateSleepPage), navigationParameter))
            {
                throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
            }
        }

        protected void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(CreateChangePage), null))
            {
                throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
            }
        }
        protected void MeasureButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(CreateMeasurementPage), null))
            {
                throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
            }
        }

        protected void AppBarSettingsButton_Click(object sender, RoutedEventArgs e)
        {
#if WINDOWS_PHONE_APP
            if (!Frame.Navigate(typeof(SettingsPage), null))
                throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
#else
            SettingsPane.Show();
#endif
        }

        protected void AppBarNfcButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!Frame.Navigate(typeof(ManageNfcTagsPage), null))
            //    throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
        }

        protected void AppBarEditBabyButton_Click(object sender, RoutedEventArgs e)
        {
            var navigationParameter = new ViewModelNavigationParameter<BabyViewModel>((BabyViewModel)DefaultViewModel.CurrentBaby);
            if (!Frame.Navigate(typeof(CreateBabyPage), navigationParameter))
                throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
        }

        protected void ActivityHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(ActivityHistory), null))
                throw new Exception(resourceLoader.GetString(Constants.RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED));
        }

        protected void BabysAgeTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CurrentBaby.ShowAgeInWeeks = SettingsViewModel.Current.ShowAgeInWeeks = !SettingsViewModel.Current.ShowAgeInWeeks;
        }
        #endregion

        private void DefaultViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentBaby"
                && DefaultViewModel.CurrentBaby != null)
                SettingsViewModel.Current.MostRecentlyAccessedBabyId = DefaultViewModel.CurrentBaby.Id;
        }
    }
}
