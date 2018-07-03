namespace FeedTime
{
    using System.Text;
    using FeedTime.Common.DataModel;
    using FeedTime.Strings;
    using Windows.ApplicationModel;
    using Windows.Networking.Proximity;
    using Windows.Storage.Streams;

    public class NfcService
    {
        public static void WriteFeedTag(string babyId)
        {
            WriteTag(GenerateAppLaunchArguments(ActivityType.Feed,
                                                babyId));
        }
        public static void WriteSleepTag(string babyId)
        {
            WriteTag(GenerateAppLaunchArguments(ActivityType.Sleep,
                                                babyId));
        }

        public static void WriteChangeTag(string babyId)
        {
            WriteTag(GenerateAppLaunchArguments(ActivityType.Change,
                                                babyId));
        }

        private static string GenerateAppLaunchArguments(ActivityType activityType, string babyId)
        {
            var launchArgsBuilder = new StringBuilder();
            launchArgsBuilder.AppendFormat(Constants.NAVIGATION_ARGUMENT_ACTIVITY,
                                           activityType);
            launchArgsBuilder.Append("&");
            launchArgsBuilder.AppendFormat(Constants.NAVIGATION_ARGUMENT_BABYID,
                                           babyId);
            return launchArgsBuilder.ToString();
        }

        private static void WriteTag(string launchArgs)
        {
            var proximityDevice = ProximityDevice.GetDefault();
            if (proximityDevice == null) return;

            string applicationLaunchMessage = string.Format("{0}\tWindows\t{1}!{2}",
                                                            launchArgs,
                                                            Package.Current.Id.FamilyName,
                                                            "App");

            using (var dataWriter = new DataWriter())
            {
                dataWriter.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf16LE;
                dataWriter.WriteString(applicationLaunchMessage);
                var launchAppPubId = proximityDevice.PublishBinaryMessage("LaunchApp:WriteTag", dataWriter.DetachBuffer());
            }
        }
    }
}