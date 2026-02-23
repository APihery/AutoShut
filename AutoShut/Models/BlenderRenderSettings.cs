namespace AutoShut.Models;

public class BlenderRenderSettings
{
    public string OutputPath { get; set; } = string.Empty;
    public int FrameStart { get; set; }
    public int FrameEnd { get; set; }
    public string FileExtension { get; set; } = ".png";
    public bool IsAnimation => FrameEnd > FrameStart;
    public int TotalFrames => Math.Max(0, FrameEnd - FrameStart + 1);
}
