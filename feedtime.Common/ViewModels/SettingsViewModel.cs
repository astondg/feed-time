namespace FeedTime.ViewModels
{
    using System;
    using System.Runtime.CompilerServices;
    using Windows.Storage;

    public class SettingsViewModel : BaseViewModel
    {
        private static readonly string DEFAULT_USERID = string.Empty;
        private static readonly string DEFAULT_MOSTRECENTLYACCESSEDBABYID = string.Empty;
        private static readonly string DEFAULT_ACTIVITYID = null;
        private static readonly string DEFAULT_FAMILYID = string.Empty;
        private static readonly bool DEFAULT_USEBACKGROUNDTASKS = true;
        private static readonly DateTime DEFAULT_LASTACTIVITYSCHEDULEGENERATEDAT = DateTime.MinValue;
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

        private ApplicationDataContainer roamingSettings;

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
        }

        /// <summary>
        /// The User ID of the currently logged in Azure Mobile Services user
        /// </summary>
        public string MobileServicesUserId
        {
            get { return GetValueOrDefault(DEFAULT_USERID); }
            set { AddOrUpdateValue(value); }
        }

        /// <summary>
        /// The Mobile Services Id of the most recently accessed baby
        /// </summary>
        public string MostRecentlyAccessedBabyId
        {
            get { return GetValueOrDefault(DEFAULT_MOSTRECENTLYACCESSEDBABYID); }
            set { AddOrUpdateValue(value); }
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
            get { return GetValueOrDefault(DEFAULT_ACTIVITYID); }
            set
            {
                if (AddOrUpdateValue(value))
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
            get { return GetValueOrDefault(DEFAULT_ACTIVITYID); }
            set
            {
                if (AddOrUpdateValue(value))
                    NotifyPropertyChanged("SleepActivityIsInProgress");
            }
        }

        /// <summary>
        /// The Id of the Family to which the current user belongs
        /// </summary>
        public string FamilyId
        {
            get { return GetValueOrDefault(DEFAULT_FAMILYID); }
            set { AddOrUpdateValue(value); }
        }

        public bool UseBackgroundTasks
        {
            get { return GetValueOrDefault(DEFAULT_USEBACKGROUNDTASKS); }
            set { AddOrUpdateValue(value); }
        }

        public bool UseMetricUnits
        {
            get { return GetValueOrDefault(DEFAULT_USEMETRICUNITS); }
            set { AddOrUpdateValue(value); }
        }

        public DateTime LastActivityScheduleGeneratedAt
        {
            get { return GetValueOrDefault(DEFAULT_LASTACTIVITYSCHEDULEGENERATEDAT); }
            set { AddOrUpdateValue(value); }
        }

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool AddOrUpdateValue(Object value, [CallerMemberName] string propertyName = null)
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

        /// <summary>
        /// Get the current value of the setting, or if it is not found, set the 
        /// setting to the default setting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected T GetValueOrDefault<T>(T defaultValue, [CallerMemberName] string propertyName = null)
        {
            if (!roamingSettings.Values.ContainsKey(propertyName)) return defaultValue;
            var currentValue = (T)roamingSettings.Values[propertyName];
            return currentValue != null ? currentValue : defaultValue;
        }
    }
}
