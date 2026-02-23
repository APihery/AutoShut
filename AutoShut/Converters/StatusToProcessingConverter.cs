using AutoShut.Models;
using System.Globalization;

namespace AutoShut.Converters;

public class StatusToProcessingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RenderStatus status)
            return status == RenderStatus.Processing;
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
