using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Eventa.Converters;

public class BoolToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTrue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
                return isTrue ? parts[0] : parts[1];
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
