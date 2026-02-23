namespace AutoShut.Models;

public class RenderProgressReport
{
    public string Message { get; init; } = string.Empty;
    public int? CurrentFrame { get; init; }
    public int? TotalFrames { get; init; }
    public double? Percentage { get; init; }
}
