using System;
using System.Threading.Tasks;
using BugSense;
using BugSense.Model;
using FeedTime.Common.Factories;
using FeedTime.Common;
using FeedTime.Common.DataModel;
using FeedTime.Strings;
using FeedTime.ViewModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Authentication.Web;
using Microsoft.WindowsAzure.MobileServices;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace FeedTime
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {
        private AccessTokenRetriever accessTokenRetriever;
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Initialize BugSense
            BugSenseHandler.Instance.InitAndStartSession(new ExceptionManager(Current), "169b18fd");
            accessTokenRetriever = new AccessTokenRetriever();
            DataSourceFactory.Current = new MobileServicesDataSource(SettingsViewModel.Current.MobileServicesUserId, accessTokenRetriever, new BugSenseLogger());
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            SuspensionManager.KnownTypes.Add(typeof(FeedActivity));
            SuspensionManager.KnownTypes.Add(typeof(SleepActivity));
            SuspensionManager.KnownTypes.Add(typeof(ChangeActivity));
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.EnableNotificationQueue(true);
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Handle OnActivated event to deal with File Open/Save continuation activation kinds
        /// </summary>
        /// <param name="e">Application activated event arguments, it can be cast to a proper sub-type based on ActivationKind</param>
        protected override void OnActivated(IActivatedEventArgs e)
        {
            var continuationEventArgs = e as WebAuthenticationBrokerContinuationEventArgs;
            if (continuationEventArgs != null)
            {
                accessTokenRetriever.LoginContinuationArguments = continuationEventArgs;
                DataSourceFactory.Current.FinaliseLogin();
            }

            base.OnActivated(e);
        }
#endif

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            await CreateRootFrame(e);

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private async Task CreateRootFrame(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                // TODO: change this value to a cache size that is appropriate for your application
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                Type typeOfFirstPage = string.IsNullOrWhiteSpace(SettingsViewModel.Current.MobileServicesUserId)
                                        ? typeof(SplashPage)
                                        : await NavigationHelper.DetermineFirstPage();

                // If arguments have been passed, e.g. from an NFC tag, then navigate to the appropriate page.
                // Set up pages, like create/select baby, take precedence over NFC activation
                // but we still need to remember follow the NFC activation once set up is complete by passing
                // the existing navigation arguments
                if (typeOfFirstPage == typeof(MainPage))
                    typeOfFirstPage = NavigationHelper.DeterminePageFromNavigationArguments(e.Arguments);

#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeOfFirstPage, e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}