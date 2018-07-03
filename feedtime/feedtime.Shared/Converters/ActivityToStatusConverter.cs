namespace FeedTime.Converters
{
    using System;
    using System.Globalization;
    using FeedTime.Common.Extensions;
    using FeedTime.ViewModels;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.Xaml.Data;

    public class ActivityToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var activity = value as ActivityViewModel;
            if (activity == null) return string.Empty;

            var resourceLoader = new ResourceLoader();
            var activityDuration = (activity.EndTime ?? DateTime.Now) - activity.StartDate;

            if (activity.EndTime == null)
            {
                return activityDuration.Hours > 0
                        ? string.Format(CultureInfo.CurrentCulture,
                                        resourceLoader.GetString("Format_ActivityStatus_RunningHours"),
                                        ConvertWordToCurrentTense(activity.Name),
                                        Math.Floor(activityDuration.TotalHours),
                                        activityDuration.Minutes)
                        : string.Format(CultureInfo.CurrentCulture,
                                        resourceLoader.GetString("Format_ActivityStatus_RunningMinutes"),
                                        ConvertWordToCurrentTense(activity.Name),
                                        Math.Floor(activityDuration.TotalMinutes));
            }
            else
            {
                return string.Format(CultureInfo.CurrentCulture,
                                     resourceLoader.GetString("Format_ActivityStatus_Complete"),
                                     activity.Name,
                                     activity.StartDate.ToStringWithSystemClock("shortdate shorttime"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        private string ConvertWordToCurrentTense(string word)
        {
            switch (word)
            {
                case "change":
                    return "changing";
                case "feed":
                case "sleep":
                default:
                    return word + "ing";
            }
        }
    }
}