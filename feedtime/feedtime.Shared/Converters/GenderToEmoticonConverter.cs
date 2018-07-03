namespace FeedTime.Converters
{
    using System;
    using FeedTime.Common.DataModel;
    using Windows.UI.Xaml.Data;

    public class GenderToEmoticonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var gender = (Gender)value;
            switch (gender)
            {
                case Gender.Female:
                    return "👧";
                case Gender.Male:
                    return "👦";
                case Gender.Undecided:
                default:
                    return "👤";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var textValue = value as string;
            switch (textValue)
            {
                case "👧":
                    return Gender.Female;
                case "👦":
                    return Gender.Male;
                case "👤":
                    return Gender.Undecided;
                default:
                    throw new InvalidOperationException(textValue + " is not a valid Gender representation");
            }
        }
    }
}