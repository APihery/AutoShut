using AutoShut.Models;
using System.Globalization;

namespace AutoShut.Converters;

public class StatusToRetryVisibleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RenderStatus status)
        {
            return status == RenderStatus.Failed;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
