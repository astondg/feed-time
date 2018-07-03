namespace FeedTime.Converters
{
    using System;
    using System.Globalization;
    using FeedTime.Common.Extensions;
    using Windows.UI.Xaml.Data;

    public class DateTimeOffsetToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var culture = new CultureInfo(language);
            DateTimeOffset? item = value as DateTimeOffset?;
            if (!item.HasValue) return parameter != null ? parameter.ToString() : string.Empty;
            return item.Value.ToStringWithSystemClock("shortdate shorttime");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var culture = new CultureInfo(language);
            string valueAsString = value.ToString();
            DateTimeOffset item;
            if (DateTimeOffset.TryParse(valueAsString, culture, DateTimeStyles.None, out item))
                return item;
            else
                throw new InvalidCastException();
        }
    }
}