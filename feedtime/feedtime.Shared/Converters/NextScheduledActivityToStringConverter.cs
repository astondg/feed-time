namespace FeedTime.Converters
{
    using System;
    using System.Globalization;
    using System.Text;
    using FeedTime.Strings;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.Xaml.Data;

    public class NextScheduledActivityToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var activityState = value as Tuple<string, DateTimeOffset, bool>;
            if (activityState == null) return string.Empty;

            var resourceLoader = new ResourceLoader();
            var nextActivityDueIn = activityState.Item2 - DateTime.Now;
            var nextActivityBuilder = new StringBuilder();
            nextActivityBuilder.AppendFormat(resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYDUETO),
                                             activityState.Item1);
            if (nextActivityDueIn > TimeSpan.Zero)
            {
                string dueInFormat = nextActivityDueIn.Hours > 0
                                        ? resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYINHOURS)
                                        : resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYINMINUTES);
                nextActivityBuilder.AppendFormat(CultureInfo.CurrentCulture, dueInFormat, nextActivityDueIn.Duration());
            }
            else
            {
                string dueAgoFormat = nextActivityDueIn.Hours > 0
                                        ? resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYAGOHOURS)
                                        : resourceLoader.GetString(Constants.RESOURCEKEY_FORMAT_ACTIVITYAGOMINUTES);
                nextActivityBuilder.AppendFormat(CultureInfo.CurrentCulture, dueAgoFormat, nextActivityDueIn.Duration());
            }

            return nextActivityBuilder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}