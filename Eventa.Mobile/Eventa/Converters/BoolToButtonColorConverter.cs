using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Eventa.Converters;

public class BoolToButtonColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
        {
            return new SolidColorBrush(Color.Parse("#FFE89696"));
        }
        return new SolidColorBrush(Color.Parse("#FFA0A0A0"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}