namespace FeedTime.Converters
{
    using System;
    using System.Globalization;
    using FeedTime.Strings;
    using Windows.UI.Xaml.Data;

    public class TimeSpanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var culture = new CultureInfo(language);
            TimeSpan? item = value as TimeSpan?;
            if (!item.HasValue) return string.Empty;
            var format = parameter != null
                            ? parameter.ToString()
                            : Constants.SHORT_POSITIVE_TIME_FORMAT;
            return item.Value.ToString(format, culture);
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