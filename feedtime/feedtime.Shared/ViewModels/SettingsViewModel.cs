namespace FeedTime.ViewModels
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using FeedTime.Strings;
    using Microsoft.Band;
    using Microsoft.Band.Tiles;
    using Microsoft.Band.Tiles.Pages;
    using Windows.ApplicationModel.Resources;
    using Windows.Storage;
    using Windows.Storage.Streams;
    using Windows.UI.Xaml.Media.Imaging;

    public class SettingsViewModel : BaseViewModel
    {
        private static readonly string DEFAULT_USERID = string.Empty;
        private static readonly string DEFAULT_MOSTRECENTLYACCESSEDBABYID = string.Empty;
        private static readonly string DEFAULT_ACTIVITYID = null;
        private static readonly string DEFAULT_FAMILYID = string.Empty;
        private static readonly string DEFAULT_NEXTACTIVITYID = string.Empty;
        private static readonly string DEFAULT_LASTKNOWNAPPVERSION = string.Empty;
        private static readonly bool DEFAULT_USEBACKGROUNDTASKS = true;
        private static readonly bool DEFAULT_SHOWAGEINWEEKS = true;
        private static readonly DateTimeOffset DEFAULT_LASTACTIVITYSCHEDULEGENERATEDAT = DateTimeOffset.MinValue;
        private static readonly DateTimeOffset DEFAULT_LASTDATATRENDSGENERATEDAT = DateTimeOffset.MinValue;
        private static bool DEFAULT_USEMETRICUNITS
        {
            get
            {
                switch (System.Globalization.CultureInfo.CurrentCulture.Name)
                {
                    case "en-US":
                    case "en-GB":
                        return false;
                    default:
                        return true;
                }
            }
        }

        private static SettingsViewModel current;

        private readonly ResourceLoader resourceLoader = new ResourceLoader();

        private ApplicationDataContainer roamingSettings;
        private ApplicationDataContainer localSettings;
        private IBandInfo[] pairedMicrosoftBands;

        static SettingsViewModel()
        {
            current = new SettingsViewModel();
        }

        public static SettingsViewModel Current { get { return current; } }

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public SettingsViewModel()
        {
            // Get the settings for this application.
            roamingSettings = ApplicationData.Current.RoamingSettings;
            localSettings = ApplicationData.Current.LocalSettings;
        }

        /// <summary>
        /// The User ID of the currently logged in Azure Mobile Services user
        /// </summary>
        public string MobileServicesUserId
        {
            get { return GetRoamingValueOrDefault(DEFAULT_USERID); }
            set { AddOrUpdateRoamingValue(value); }
        }

        /// <summary>
        /// The Id of the Family to which the current user belongs
        /// </summary>
        public string FamilyId
        {
            get { return GetRoamingValueOrDefault(DEFAULT_FAMILYID); }
            set { AddOrUpdateRoamingValue(value); }
        }

        /// <summary>
        /// The Mobile Services Id of the most recently accessed baby
        /// </summary>
        public string MostRecentlyAccessedBabyId
        {
            get { return GetRoamingValueOrDefault(DEFAULT_MOSTRECENTLYACCESSEDBABYID); }
            set { AddOrUpdateRoamingValue(value); }
        }

        /// <summary>
        /// True if a feed activity is currently in progress on this device
        /// </summary>
        public bool FeedActivityIsInProgress
        {
            get { return InProgressFeedActivityId != null; }
        }

        /// <summary>
        /// The Mobile Services Id of the feed activity currently in progess or null if no activity is running
        /// </summary>
        public string InProgressFeedActivityId
        {
            get { return GetRoamingValueOrDefault(DEFAULT_ACTIVITYID); }
            set
            {
                if (AddOrUpdateRoamingValue(value))
                    NotifyPropertyChanged("FeedActivityIsInProgress");
            }
        }

        /// <summary>
        /// True if a feed activity is currently in progress on this device
        /// </summary>
        public bool SleepActivityIsInProgress
        {
            get { return InProgressSleepActivityId != null; }
        }

        /// <summary>
        /// The Mobile Services Id of the feed activity currently in progess or null if no activity is running
        /// </summary>
        public string InProgressSleepActivityId
        {
            get { return GetRoamingValueOrDefault(DEFAULT_ACTIVITYID); }
            set
            {
                if (AddOrUpdateRoamingValue(value))
                    NotifyPropertyChanged("SleepActivityIsInProgress");
            }
        }

        public bool UseMetricUnits
        {
            get { return GetRoamingValueOrDefault(DEFAULT_USEMETRICUNITS); }
            set { AddOrUpdateRoamingValue(value); }
        }

        public bool UseBackgroundTasks
        {
            get { return GetLocalValueOrDefault(DEFAULT_USEBACKGROUNDTASKS); }
            set { AddOrUpdateLocalValue(value); }
        }

        public DateTimeOffset LastActivityScheduleGeneratedAt
        {
            get { return GetLocalValueOrDefault(DEFAULT_LASTACTIVITYSCHEDULEGENERATEDAT); }
            set { AddOrUpdateLocalValue(value); }
        }

        public DateTimeOffset LastDataTrendsGeneratedAt
        {
            get { return GetLocalValueOrDefault(DEFAULT_LASTDATATRENDSGENERATEDAT); }
            set { AddOrUpdateLocalValue(value); }
        }

        public string NextFeedNotificationId
        {
            get { return GetLocalValueOrDefault(DEFAULT_NEXTACTIVITYID); }
            set { AddOrUpdateLocalValue(value); }
        }

        public string NextSleepNotificationId
        {
            get { return GetLocalValueOrDefault(DEFAULT_NEXTACTIVITYID); }
            set { AddOrUpdateLocalValue(value); }
        }

        public string NextChangeNotificationId
        {
            get { return GetLocalValueOrDefault(DEFAULT_NEXTACTIVITYID); }
            set { AddOrUpdateLocalValue(value); }
        }

        public bool ShowAgeInWeeks
        {
            get { return GetLocalValueOrDefault(DEFAULT_SHOWAGEINWEEKS); }
            set { AddOrUpdateLocalValue(value); }
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool AddOrUpdateRoamingValue(Object value, [CallerMemberName] string propertyName = null)
        {
            var currentValue = roamingSettings.Values[propertyName];
            if (currentValue == null || currentValue != value)
            {
                roamingSettings.Values[propertyName] = value;
                NotifyPropertyChanged(propertyName);
                return true;
            }
            return false;
        }

        public string LastKnownAppVersion
        {
            get { return GetLocalValueOrDefault(DEFAULT_LASTKNOWNAPPVERSION); }
            set { AddOrUpdateLocalValue(value); }
        }

        public void ClearSettings()
        {
            localSettings.Values.Clear();
#if DEBUG
            roamingSettings.Values.Clear();
#endif
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetRoamingValueOrDefault<T>(T defaultValue, [CallerMemberName] string propertyName = null)
        {
            if (!roamingSettings.Values.ContainsKey(propertyName)) return defaultValue;
            var currentValue = (T)roamingSettings.Values[propertyName];
            return currentValue != null ? currentValue : defaultValue;
        }

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetLocalValueOrDefault<T>(T defaultValue, [CallerMemberName] string propertyName = null)
        {
            if (!localSettings.Values.ContainsKey(propertyName)) return defaultValue;
            var currentValue = (T)localSettings.Values[propertyName];
            return currentValue != null ? currentValue : defaultValue;
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool AddOrUpdateLocalValue(Object value, [CallerMemberName] string propertyName = null)
        {
            var currentValue = localSettings.Values[propertyName];
            if (currentValue == null || currentValue != value)
            {
                localSettings.Values[propertyName] = value;
                NotifyPropertyChanged(propertyName);
                return true;
            }
            return false;
        }
        
        private async Task FindPairedMicrosoftBands()
        {
            pairedMicrosoftBands = await BandClientManager.Instance.GetBandsAsync();
        }

        private async Task RegisterMicrosftBandApp()
        {
            if (pairedMicrosoftBands.Length < 1)
                throw new InvalidOperationException("No paired Microsoft Band");

            using (var bandClient = await Microsoft.Band.BandClientManager.Instance.ConnectAsync(pairedMicrosoftBands[0]))
            {
                var tile = new Microsoft.Band.Tiles.BandTile(new Guid("/*TODO*/"))
                {
                    Name = resourceLoader.GetString(Constants.RESOURCEKEY_APPNAME),
                    TileIcon = await LoadIcon(""),
                    SmallIcon = await LoadIcon("")
                };

                TextButton button = new TextButton
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    ElementId = 1, Rect = new PageRect(10, 10, 200, 90)
                };
                FilledPanel panel = new FilledPanel(button) { Rect = new PageRect(0, 0, 220, 150) };
                tile.PageLayouts.Add(new PageLayout(panel));

                var tileWasAdded = await bandClient.TileManager.AddTileAsync(tile);
            }
        }

        private async Task<BandIcon> LoadIcon(string uri)
        {
            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));

            using (IRandomAccessStream fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
            {
                WriteableBitmap bitmap = new WriteableBitmap(1, 1);
                await bitmap.SetSourceAsync(fileStream);
                return bitmap.ToBandIcon();
            }
        }
    }
}
