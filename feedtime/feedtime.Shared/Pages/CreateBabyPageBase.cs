namespace FeedTime.Pages
{
    using System;
    using FeedTime.Common;
    using FeedTime.Extensions;
    using FeedTime.ViewModels;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Navigation;

    public class CreateBabyPageBase : Page
    {
        private NavigationHelper navigationHelper;
        private BabyViewModel viewModel;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView();

        public CreateBabyPageBase()
        {
            viewModel = new BabyViewModel();
            viewModel.BabyCreated += viewModel_BabyCreated;
            this.navigationHelper = new NavigationHelper(this);
        }

        /// <summary>
        /// True if DefaultViewModel is an existing baby (i.e. exists on the server)
        /// </summary>
        public bool IsExistingBaby { get; set; }

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
        /// Gets the view model for this <see cref="Page"/>. This can be changed to a strongly typed view model.
        /// </summary>
        public BabyViewModel DefaultViewModel
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
        protected virtual void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            var navigationParameter = e.NavigationParameter as ViewModelNavigationParameter<BabyViewModel>;

            // If we have been passed an existing baby then display it
            if (navigationParameter != null && navigationParameter.ViewModel != null)
            {
                IsExistingBaby = true;
                viewModel.From(navigationParameter.ViewModel);
            }
            // Otherwise prepare a new baby
            else
            {
                IsExistingBaby = false;
                DefaultViewModel.Family = new FamilyViewModel { Id = SettingsViewModel.Current.FamilyId };
                DefaultViewModel.DateOfBirth = DateTimeOffset.Now.Date;
                DefaultViewModel.TimeOfBirth = DateTimeOffset.Now.TimeOfDay;
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

        private void viewModel_BabyCreated(BabyViewModel sender, string args)
        {
            SettingsViewModel.Current.MostRecentlyAccessedBabyId = args;
            if (!Frame.Navigate(typeof(MainPage), null))
                throw new Exception("Failed to create initial page");
        }
    }
}