namespace FeedTime.BackgroundTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using FeedTime.Common.DataModel;
    using Windows.Storage;
    using Windows.System.UserProfile;

    public sealed class BackgroundSettingsViewModel
    {
        private static readonly string DEFAULT_USERID = string.Empty;
        private static readonly string DEFAULT_MOSTRECENTLYACCESSEDBABYID = string.Empty;
        private static readonly string DEFAULT_FAMILYID = string.Empty;
        private static readonly string DEFAULT_NEXTACTIVITYID = string.Empty;
        private static readonly DateTimeOffset DEFAULT_LASTACTIVITYSCHEDULEGENERATEDAT = DateTimeOffset.MinValue;
        private static readonly Dictionary<ActivityType, string> DEFAULT_NEXTACTIVITYNOTIFICATIONS
            = new Dictionary<ActivityType, string>();
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

        private ApplicationDataContainer roamingSettings;
        private ApplicationDataContainer localSettings;

        /// <summary>
        /// Constructor that gets the application settings.
        /// </summary>
        public BackgroundSettingsViewModel()
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

        public bool UseMetricUnits
        {
            get { return GetRoamingValueOrDefault(DEFAULT_USEMETRICUNITS); }
            set { AddOrUpdateRoamingValue(value); }
        }

        public DateTimeOffset LastActivityScheduleGeneratedAt
        {
            get { return GetLocalValueOrDefault(DEFAULT_LASTACTIVITYSCHEDULEGENERATEDAT); }
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

        /// <summary>
        /// Update a setting value for our application. If the setting does not
        /// exist, then add the setting.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool AddOrUpdateRoamingValue(Object value, [CallerMemberName] string propertyName = null)
        {
            var currentValue = roamingSettings.Values[propertyName];
            if (currentValue == null || currentValue != value)
            {
                roamingSettings.Values[propertyName] = value;
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
        private T GetRoamingValueOrDefault<T>(T defaultValue, [CallerMemberName] string propertyName = null)
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
        private T GetLocalValueOrDefault<T>(T defaultValue, [CallerMemberName] string propertyName = null)
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
        private bool AddOrUpdateLocalValue(Object value, [CallerMemberName] string propertyName = null)
        {
            var currentValue = localSettings.Values[propertyName];
            if (currentValue == null || currentValue != value)
            {
                localSettings.Values[propertyName] = value;
                return true;
            }
            return false;
        }
    }
}
