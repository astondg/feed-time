namespace FeedTime.Pages
{
    using System;
    using System.Linq;
    using FeedTime.Common;
    using FeedTime.Extensions;
    using FeedTime.Strings;
    using FeedTime.ViewModels;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.Notifications;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public class CreateChangePageBase : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();
        private readonly ChangeActivityViewModel viewModel;
        private readonly string activityName;

        public CreateChangePageBase()
        {
            viewModel = new ChangeActivityViewModel();
            viewModel.ActivityUpdated += viewModel_ActivityUpdated;
            this.navigationHelper = new NavigationHelper(this);
            activityName = resourceLoader.GetString(Constants.RESOURCEKEY_ACTIVITYNAME_CHANGE);
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
        public ChangeActivityViewModel DefaultViewModel
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
        protected void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var settings = SettingsViewModel.Current;
            DefaultViewModel.StartDate = DateTime.Now;
            DefaultViewModel.EndTime = DateTime.Now;
            DefaultViewModel.Baby = new BabyViewModel { Id = settings.MostRecentlyAccessedBabyId };
            DefaultViewModel.NappiesUsed = 1;
            DefaultViewModel.WipesUsed = 2;    

            var navigationParameter = e.NavigationParameter as ViewModelNavigationParameter<ChangeActivityViewModel>;

            if (navigationParameter != null)
            {
                if (Constants.NAVIGATION_PARAMETER_NFCINITIATED.Equals(navigationParameter.Flag))
                    navigationParameter.ViewModel.NfcInitiated = true;

                // If we have been passed an existing change then display it
                if (navigationParameter.ViewModel != null)
                    viewModel.From(navigationParameter.ViewModel);
            }
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
        { }

        private void viewModel_ActivityUpdated(ChangeActivityViewModel sender, string args)
        {
            var settings = SettingsViewModel.Current;
            
            // We've just created a new change which invalidates any scheduled change 
            // notifications so delete them
            if (!string.IsNullOrWhiteSpace(settings.NextChangeNotificationId))
            {
                var toastNotifier = ToastNotificationManager.CreateToastNotifier();
                var scheduledNotificaiton = toastNotifier.GetScheduledToastNotifications()
                                                         .SingleOrDefault(tn => tn.Id == settings.NextChangeNotificationId);
                if (scheduledNotificaiton != null)
                    toastNotifier.RemoveFromSchedule(scheduledNotificaiton);
            }

            settings.LastActivityScheduleGeneratedAt = DateTimeOffset.MinValue;
            settings.LastDataTrendsGeneratedAt = DateTimeOffset.MinValue;
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