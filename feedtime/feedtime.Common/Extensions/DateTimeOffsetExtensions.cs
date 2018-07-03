namespace FeedTime.Common.Extensions
{
    using System;
    using System.Linq;
    using Windows.Foundation.Metadata;
    using Windows.Globalization.DateTimeFormatting;
    using Windows.System.UserProfile;

    public static class DateTimeOffsetExtensions
    {
        [DefaultOverload]
        public static string ToStringWithSystemClock(this DateTimeOffset dateTime, string formatTemplate)
        {
            var defaultFormatter = new DateTimeFormatter(formatTemplate);
            var formatter = new DateTimeFormatter(formatTemplate,
                                                  defaultFormatter.Languages,
                                                  defaultFormatter.GeographicRegion,
                                                  defaultFormatter.Calendar,
                                                  GlobalizationPreferences.Clocks.FirstOrDefault());
            return formatter.Format(dateTime);
        }

        public static string ToStringWithSystemClock(this DateTimeOffset? dateTime, string formatTemplate)
        {
            return dateTime.HasValue
                    ? dateTime.Value.ToStringWithSystemClock(formatTemplate)
                    : null;
        }
    }
}