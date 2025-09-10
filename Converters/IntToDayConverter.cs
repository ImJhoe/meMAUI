using System.Globalization;

namespace ClinicaApp.Converters
{
    public class IntToDayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int day && day >= 1 && day <= 7)
            {
                return day - 1; // Picker es base 0
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index + 1; // Convertir de base 0 a base 1
            }
            return 1;
        }
    }
}