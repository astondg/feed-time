namespace FeedTime.Converters
{
    using System;
    using System.Globalization;
    using FeedTime.Strings;
    using Windows.ApplicationModel.Resources;
    using Windows.UI.Xaml.Data;

    public class TimeSpanToGreatestUnitStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var culture = new CultureInfo(language);
            TimeSpan? item = value as TimeSpan?;
            if (!item.HasValue) return string.Empty;
            var resourceLoader = new ResourceLoader();
            return item.Value.Hours > 0
                    ? string.Format("{0} {1}", item.Value.Hours, resourceLoader.GetString(Constants.RESOURCEKEY_UNIT_TIME_HOURS))
                    : string.Format("{0} {1}", item.Value.Minutes, resourceLoader.GetString(Constants.RESOURCEKEY_UNIT_TIME_MINUTES));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var culture = new CultureInfo(language);
            TimeSpan item;
            if (TimeSpan.TryParse(value.ToString(), culture, out item))
                return item;
            else
                throw new InvalidCastException();
        }
    }
}