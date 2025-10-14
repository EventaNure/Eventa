using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Eventa.Converters;

public class StringNotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool result = !string.IsNullOrWhiteSpace(value as string);
        if (parameter is string p && p.Equals("Invert", StringComparison.OrdinalIgnoreCase))
            result = !result;
        return result;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}