namespace FeedTime.Strings
{
    public static class Constants
    {
        public const string URL_MOBILESERVICES = "https://feedtime.azure-mobile.net/";
        public const string OAUTH_APPKEY_MOBILESERVICES = "EGihCnXhHDboThXWVOUcWVyUZSYWrD66";

        public const string MICROSOFT_LIVE_SCOPE_BASIC = "wl.basic";
        public const string MICROSOFT_LIVE_SCOPE_SIGNIN = "wl.signin";
        public const string MICROSOFT_LIVE_SCOPE_OFFLINEACCESS = "wl.offline_access";

        public const string LONG_POSITIVE_TIME_FORMAT = @"h\:mm\:ss\.fff";
        public const string SHORT_POSITIVE_TIME_FORMAT = @"h\:mm\:ss";

        public const string NAVIGATION_PARAMETER_CLEARLASTBACKENTRY = "clearLastBackEntry";
        public const string NAVIGATION_PARAMETER_NFCINITIATED = "nfcInitiated";
        public const string NAVIGATION_ARGUMENT_ACTIVITY = "activity={0}";
        public const string NAVIGATION_ARGUMENT_BABYID = "babyId={0}";

        public const string APP_BACKGROUNDTASK_NAME = "ActivityReminderBackgroundTask";
        public const string APP_BACKGROUNDTASK_ENTRYPOINT = "FeedTime.BackgroundTasks.ActivityReminderBackgroundTask";

        public const string RESOURCEKEY_ACTIVITYNAME_FEED = "ActivityName_Feed";
        public const string RESOURCEKEY_ACTIVITYNAME_SLEEP = "ActivityName_Sleep";
        public const string RESOURCEKEY_ACTIVITYNAME_CHANGE = "ActivityName_Change";
        public const string RESOURCEKEY_LOADINGSTATUS_DEFAULT = "LoadingStatus_Default";
        public const string RESOURCEKEY_LOADINGSTATUS_BABY = "LoadingStatus_Baby";
        public const string RESOURCEKEY_LOADINGSTATUS_SCHEDULE = "LoadingStatus_Schedule";
        public const string RESOURCEKEY_LOADINGSTATUS_FEED = "LoadingStatus_Feed";
        public const string RESOURCEKEY_LOADINGSTATUS_SLEEP = "LoadingStatus_Sleep";
        public const string RESOURCEKEY_LOADINGSTATUS_BABIES = "LoadingStatus_Babies";
        public const string RESOURCEKEY_LOADINGSTATUS_MEASUREMENTS = "LoadingStatus_Measurements";
        public const string RESOURCEKEY_LOADINGSTATUS_RHYTHM = "LoadingStatus_Rhythm";
        public const string RESOURCEKEY_LOADINGSTATUS_UPDATINGSERVER = "LoadingStatus_UpdatingServer";
        public const string RESOURCEKEY_LOADINGSTATUS_LOGIN = "LoadingStatus_Login";
        public const string RESOURCEKEY_UNIT_WEIGHT_METRIC = "Unit_Weight_Metric";
        public const string RESOURCEKEY_UNIT_WEIGHT_IMPERIAL = "Unit_Weight_Imperial";
        public const string RESOURCEKEY_UNIT_LENGTH_METRIC = "Unit_Length_Metric";
        public const string RESOURCEKEY_UNIT_LENGTH_IMPERIAL = "Unit_Length_Imperial";
        public const string RESOURCEKEY_UNIT_TIME_HOURS = "Unit_Time_Hours";
        public const string RESOURCEKEY_UNIT_TIME_MINUTES = "Unit_Time_Minutes";
        public const string RESOURCEKEY_DIALOG_BACKGROUNDTASKSDISABLED = "Dialog_BackgroundTasksDisabled";
        public const string RESOURCEKEY_DIALOG_LOGINNOTCOMPLETED = "Dialog_LoginNotCompleted";
        public const string RESOURCEKEY_FORMAT_ACTIVITYBUTTONHOURS = "Format_ActivityButtonHours";
        public const string RESOURCEKEY_FORMAT_ACTIVITYBUTTONMINUTES = "Format_ActivityButtonMinutes";
        public const string RESOURCEKEY_FORMAT_ACTIVITYDUETO = "Format_ActivityDueTo";
        public const string RESOURCEKEY_FORMAT_ACTIVITYINHOURS = "Format_ActivityInHours";
        public const string RESOURCEKEY_FORMAT_ACTIVITYINMINUTES = "Format_ActivityInMinutes";
        public const string RESOURCEKEY_FORMAT_ACTIVITYAGOHOURS = "Format_ActivityAgoHours";
        public const string RESOURCEKEY_FORMAT_ACTIVITYAGOMINUTES = "Format_ActivityAgoMinutes";
        public const string RESOURCEKEY_EXCEPTION_NAVIGATIONFAILED = "Exception_NavigationFailed";
        public const string RESOURCEKEY_BLURB_WELCOMETOFEEDTIME = "Blurb_WelcomeToFeedTime";
        public const string RESOURCEKEY_APPNAME = "AppName";

        public const double UNITCONVERSION_MILLILITRES_OUNCES = 0.03519506;
        public const double UNITCONVERSION_CENTIMETRES_INCHES = 0.39370079;
        public const double UNITCONVERSION_KILOGRAMS_POUNDS = 2.20462262;

        public const string UNIT_SUFFIX_MILLILITRES = "ml";
        public const string UNIT_SUFFIX_OUNCES = "oz";
    }
}