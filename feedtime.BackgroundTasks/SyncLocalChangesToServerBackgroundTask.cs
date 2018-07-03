namespace FeedTime.BackgroundTasks
{
    using System;
    using FeedTime.Common.DataModel;
    using Windows.ApplicationModel.Background;
    using Windows.Networking.Connectivity;

    public sealed class SyncLocalChangesToServerBackgroundTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            NetworkStateChangeEventDetails details = taskInstance.TriggerDetails as NetworkStateChangeEventDetails;
            if (!details.HasNewNetworkConnectivityLevel) return;

            var connectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (connectionProfile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
                return;

            var deferral = taskInstance.GetDeferral();

            var settings = new BackgroundSettingsViewModel();
            var dataSource = new MobileServicesDataSource(settings.MobileServicesUserId,
                                                            new AccessTokenRetriever(),
                                                            new BugSenseLogger());
            bool couldLogIn = await dataSource.Login(false, true);
            if (couldLogIn)
                await dataSource.PushLocalChangesToServer();

            deferral.Complete();
        }
    }
}