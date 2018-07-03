namespace FeedTime.Common
{
    using System;
    using System.Threading.Tasks;
    using FeedTime.Extensions;
    using FeedTime.Strings;
    using FeedTime.ViewModels;
    using Windows.ApplicationModel;
    using Windows.ApplicationModel.Background;
    using Windows.UI.Popups;

    public static class BackgroundTaskRegistrar
    {
        public static async Task RegisterBackgroundTask(string failureMessage)
        {
            if (!SettingsViewModel.Current.UseBackgroundTasks)
                return;

            // If the application has been updated since the last run then remove access
            String appVersion = Package.Current.GetAppVersionNumber();
            if (!string.Equals(SettingsViewModel.Current.LastKnownAppVersion, appVersion))
                BackgroundExecutionManager.RemoveAccess();

            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.Denied
                || backgroundAccessStatus == BackgroundAccessStatus.Unspecified)
            {
                await new MessageDialog(failureMessage).ShowAsync();
                SettingsViewModel.Current.UseBackgroundTasks = false;
            }
            else
            {
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == Constants.APP_BACKGROUNDTASK_NAME)
                    {
                        task.Value.Unregister(true);
                        break;
                    }
                }

                var builder = new BackgroundTaskBuilder();
                builder.Name = Constants.APP_BACKGROUNDTASK_NAME;
                builder.TaskEntryPoint = Constants.APP_BACKGROUNDTASK_ENTRYPOINT;
                builder.SetTrigger(new TimeTrigger(15, false));
                builder.Register();
            }
        }
    }
}