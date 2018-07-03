namespace FeedTime.BackgroundTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FeedTime.Common.DataModel;
    using FeedTime.Common.Extensions;
    using Windows.ApplicationModel.Background;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.Notifications;

    public sealed class ActivityReminderBackgroundTask : IBackgroundTask
    {
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            tileUpdater.EnableNotificationQueue(true);
            tileUpdater.Clear();

            var settings = new BackgroundSettingsViewModel();
            var dataSource = new MobileServicesDataSource(settings.MobileServicesUserId,
                                                          new AccessTokenRetriever(),
                                                          new BugSenseLogger());
            bool couldLogIn = await dataSource.Login(false, true);
            if (couldLogIn)
            {
                var babies = await dataSource.GetBabies(settings.FamilyId);
                var babyActiviyScheduleTasks = babies.Select(async baby =>
                    {
                        var activitySchedule = await dataSource.GetActivitySchedule(baby.Id);
                        ScheduleToastNotifications(activitySchedule, baby, settings);
                        return Tuple.Create(baby, activitySchedule);
                    });
                var babyActivitySchedules = await Task.WhenAll(babyActiviyScheduleTasks.ToArray());
                UpdateTiles(babyActivitySchedules, tileUpdater, settings);
            }

            deferral.Complete();
        }

        private void UpdateTiles(IEnumerable<Tuple<Baby, ActivitySchedule>> schedules, TileUpdater tileUpdater, BackgroundSettingsViewModel settings)
        {
            string feedActivityName = resourceLoader.GetString("ActivityName_Feed"),
                   sleepActivityName = resourceLoader.GetString("ActivityName_Sleep"),
                   changeActivityName = resourceLoader.GetString("ActivityName_Change");

            var feedSchedules = schedules.Select(s => new BabyActivityStatus
                {
                    ActivityName = feedActivityName,
                    BabyGivenName = s.Item1.GivenName,
                    ActivityIsRunning = s.Item2.CurrentlyFeeding,
                    LastActivityStartTime = s.Item2.LastFeed != null ? s.Item2.LastFeed.StartTime : (DateTimeOffset?)null,
                    NextActivityStartTime = s.Item2.NextFeedDueAt
                });

            var sleepSchedules = schedules.Select(s => new BabyActivityStatus
                {
                    ActivityName = sleepActivityName,
                    BabyGivenName = s.Item1.GivenName,
                    ActivityIsRunning = s.Item2.CurrentlySleeping,
                    LastActivityStartTime = s.Item2.LastSleep != null ? s.Item2.LastSleep.StartTime : (DateTimeOffset?)null,
                    NextActivityStartTime = s.Item2.NextSleepDueAt
                });

            var changeSchedules = schedules.Select(s => new BabyActivityStatus
                {
                    ActivityName = changeActivityName,
                    BabyGivenName = s.Item1.GivenName,
                    ActivityIsRunning = false,
                    LastActivityStartTime = s.Item2.LastChange != null ? s.Item2.LastChange.StartTime : (DateTimeOffset?)null,
                    NextActivityStartTime = s.Item2.NextChangeDueAt
                });

            tileUpdater.CreateActivityTile(feedActivityName, feedSchedules);
            tileUpdater.CreateActivityTile(sleepActivityName, sleepSchedules);
            tileUpdater.CreateActivityTile(changeActivityName, changeSchedules);
        }

        private static void ScheduleToastNotifications(ActivitySchedule schedule, Baby baby, BackgroundSettingsViewModel settings)
        {
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();
            string notificationId;

            // Delete & recreate Feed notification
            DeleteExistingNotification(toastNotifier, settings.NextFeedNotificationId);
            settings.NextFeedNotificationId = null;
            if (!schedule.CurrentlyFeeding)
            {
                notificationId = ScheduleToastNotificationForActivity(schedule.NextFeedDueAt,
                                                                          "feed",
                                                                          baby.GivenName,
                                                                          toastNotifier);
                settings.NextFeedNotificationId = notificationId;
            }

            // Delete & recreate Sleep notification
            DeleteExistingNotification(toastNotifier, settings.NextSleepNotificationId);
            settings.NextSleepNotificationId = null;
            if (!schedule.CurrentlySleeping)
            {
                notificationId = ScheduleToastNotificationForActivity(schedule.NextSleepDueAt,
                                                                      "sleep",
                                                                      baby.GivenName,
                                                                      toastNotifier);
                settings.NextSleepNotificationId = notificationId;
            }

            // Delete & recreate Change notification
            DeleteExistingNotification(toastNotifier, settings.NextChangeNotificationId);
            settings.NextChangeNotificationId = null;
            notificationId = ScheduleToastNotificationForActivity(schedule.NextChangeDueAt,
                                                                  "change",
                                                                  baby.GivenName,
                                                                  toastNotifier);
            settings.NextChangeNotificationId = notificationId;
        }

        private static void DeleteExistingNotification(ToastNotifier toastNotifier, string notificationId)
        {
            if (string.IsNullOrWhiteSpace(notificationId)) return;
            var scheduledNotificaitons = toastNotifier.GetScheduledToastNotifications();
            var existingNotificaiton = scheduledNotificaitons.SingleOrDefault(n => n.Id == notificationId);
            if (existingNotificaiton != null)
                toastNotifier.RemoveFromSchedule(existingNotificaiton);
        }

        private static string ScheduleToastNotificationForActivity(DateTimeOffset nextActivityTime, string activityName, string babysGivenName, ToastNotifier toastNotifier)
        {
            if (nextActivityTime <= DateTimeOffset.Now)
                return null;

            var notificationText = string.Format("{0} is due to {1}", babysGivenName, activityName);
            return ScheduleToastNotification(notificationText, nextActivityTime, activityName + babysGivenName, toastNotifier);
        }

        private static string ScheduleToastNotification(string text, DateTimeOffset notificationTime, string notificationId, ToastNotifier toastNotifier)
        {
            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
            toastXml.GetElementsByTagName("text")[0].AppendChild(toastXml.CreateTextNode(text));
            var toastNotificaiton = new ScheduledToastNotification(toastXml, notificationTime);
            toastNotificaiton.Id = notificationId;
            toastNotifier.AddToSchedule(toastNotificaiton);
            return toastNotificaiton.Id;
        }
    }
}