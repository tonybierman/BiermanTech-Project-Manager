using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace BiermanTech.ProjectManager.Converters;

public class BoolToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isHovering && isHovering)
        {
            return new SolidColorBrush(Colors.LightGray);
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}