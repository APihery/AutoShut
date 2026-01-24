using AutoShut.Models;
using System.Globalization;

namespace AutoShut.Converters;

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not RenderStatus status) return Color.FromArgb("#6B7280");
        
        return status switch
        {
            RenderStatus.Pending => Color.FromArgb("#6B7280"),
            RenderStatus.Processing => Color.FromArgb("#3B82F6"),
            RenderStatus.Completed => Color.FromArgb("#10B981"),
            RenderStatus.Failed => Color.FromArgb("#EF4444"),
            RenderStatus.Cancelled => Color.FromArgb("#F59E0B"),
            _ => Color.FromArgb("#6B7280")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
