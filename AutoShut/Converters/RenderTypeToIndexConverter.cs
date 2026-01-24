using AutoShut.Models;
using System.Globalization;

namespace AutoShut.Converters;

public class RenderTypeToIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RenderType renderType)
        {
            return renderType == RenderType.Image ? 0 : 1;
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
        {
            return index == 0 ? RenderType.Image : RenderType.Animation;
        }
        return RenderType.Image;
    }
}
