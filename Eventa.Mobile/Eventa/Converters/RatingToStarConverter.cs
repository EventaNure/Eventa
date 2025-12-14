using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Eventa.Converters;

public class RatingToStarConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int rating && parameter is string starPositionStr && int.TryParse(starPositionStr, out int starPosition))
        {
            // Star position is 1-based (1, 2, 3, 4, 5)
            // If rating is >= star position, fill the star
            if (rating >= starPosition)
            {
                return new SolidColorBrush(Color.Parse("#FFE89696")); // Filled star
            }
            else
            {
                return new SolidColorBrush(Color.Parse("#FFE0E0E0")); // Empty star
            }
        }

        return new SolidColorBrush(Color.Parse("#FFE0E0E0")); // Default empty star
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}