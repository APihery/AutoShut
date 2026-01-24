using AutoShut.Models;
using System.Globalization;

namespace AutoShut.Converters;

public class StatusToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is RenderStatus status)
        {
            return status switch
            {
                RenderStatus.Pending => "Pending",
                RenderStatus.Processing => "Processing",
                RenderStatus.Completed => "Completed",
                RenderStatus.Failed => "Failed",
                RenderStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }
        return "Unknown";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
