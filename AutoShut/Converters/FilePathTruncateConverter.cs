using System.Globalization;
using System.IO;

namespace AutoShut.Converters;

public class FilePathTruncateConverter : IValueConverter
{
    private const int MaxLength = 50;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string filePath || string.IsNullOrEmpty(filePath))
            return string.Empty;

        if (filePath.Length <= MaxLength)
            return filePath;

        var fileName = Path.GetFileName(filePath);
        var directory = Path.GetDirectoryName(filePath);
        
        if (string.IsNullOrEmpty(directory))
            return fileName;

        if (fileName.Length > MaxLength)
        {
            return "..." + fileName.Substring(fileName.Length - MaxLength + 3);
        }

        var availableLength = MaxLength - fileName.Length - 5;
        if (availableLength > 0 && directory.Length > availableLength)
        {
            var start = directory.Substring(0, availableLength / 2);
            var end = directory.Substring(directory.Length - availableLength / 2);
            return $"{start}...{end}\\{fileName}";
        }

        return $"{directory}\\{fileName}";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
