using AutoShut.Models;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoShut.Services;

public class BlenderRenderer : IBlenderRenderer
{
    private readonly IBlenderSettingsExtractor _settingsExtractor;

    private const string BlenderExecutableName = "blender.exe";
    private readonly string[] _commonBlenderPaths = new[]
    {
        @"C:\Program Files\Blender Foundation\Blender *\blender.exe",
        @"C:\Program Files (x86)\Blender Foundation\Blender *\blender.exe",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Blender Foundation\Blender *\blender.exe"
    };

    // Patterns to detect frame progress from Blender stdout (format varies by version)
    private static readonly Regex SavedFrameRegex = new(@"Saved:\s*['""]?[^'""]*?(\d{3,})[^'""]*['""]?", RegexOptions.Compiled);
    private static readonly Regex FrameLineRegex = new(@"Frame\s+(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BracketedFrameRegex = new(@"\[?\s*(\d+)\s*/\s*(\d+)\s*\]?", RegexOptions.Compiled);

    public BlenderRenderer(IBlenderSettingsExtractor settingsExtractor)
    {
        _settingsExtractor = settingsExtractor;
    }

    public async Task<bool> RenderAsync(BlenderFile blenderFile, IProgress<RenderProgressReport>? progress = null, CancellationToken cancellationToken = default)
    {
#if WINDOWS || MACCATALYST
        try
        {
            var blenderPath = GetBlenderExecutablePath();
            if (string.IsNullOrEmpty(blenderPath))
            {
                Report(progress, "Blender is not installed or not found");
                return false;
            }

            if (!File.Exists(blenderFile.FilePath))
            {
                Report(progress, $"File {blenderFile.FileName} does not exist");
                return false;
            }

            Report(progress, $"Starting render of {blenderFile.FileName}...");

            BlenderRenderSettings? settings = null;
            try
            {
                settings = await _settingsExtractor.ExtractAsync(blenderFile.FilePath, blenderPath, cancellationToken);
            }
            catch
            {
                // Fallback to default behavior if extraction fails
            }

            var arguments = BuildBlenderArguments(blenderFile, settings);
            var workingDir = Path.GetDirectoryName(blenderFile.FilePath) ?? "";
            var outputPath = ResolveOutputPath(blenderFile, settings, workingDir);

            if (settings != null)
            {
                blenderFile.TotalFrames = settings.TotalFrames;
                blenderFile.CurrentFrame = 0;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = blenderPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDir
            };

            using var process = new Process { StartInfo = processStartInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();
            var lastReportedFrame = 0;

            void OnOutput(string? data)
            {
                if (string.IsNullOrEmpty(data)) return;
                outputBuilder.AppendLine(data);

                var frame = TryParseFrameFromOutput(data, blenderFile.TotalFrames);
                if (frame.HasValue && frame.Value > lastReportedFrame)
                {
                    lastReportedFrame = frame.Value;
                    blenderFile.CurrentFrame = frame.Value;
                    var pct = blenderFile.TotalFrames > 0
                        ? (double)frame.Value / blenderFile.TotalFrames * 100
                        : 0;
                    blenderFile.RenderProgressPercentage = pct;
                    Report(progress, $"Frame {frame.Value}/{blenderFile.TotalFrames}", frame.Value, blenderFile.TotalFrames, pct);
                }
                else
                {
                    Report(progress, data);
                }
            }

            process.OutputDataReceived += (_, e) => OnOutput(e.Data);
            process.ErrorDataReceived += (_, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                errorBuilder.AppendLine(e.Data);
                OnOutput(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            blenderFile.CurrentFrame = 0;
            blenderFile.RenderProgressPercentage = 0;

            if (process.ExitCode == 0)
            {
                blenderFile.OutputPath = outputPath;
                Report(progress, $"Render completed: {blenderFile.FileName}");
                return true;
            }

            blenderFile.ErrorMessage = errorBuilder.ToString();
            Report(progress, $"Error during render: {blenderFile.FileName}");
            return false;
        }
        catch (Exception ex)
        {
            blenderFile.ErrorMessage = ex.Message;
            Report(progress, $"Exception: {ex.Message}");
            return false;
        }
#else
        await Task.CompletedTask;
        Report(progress, "Blender rendering not supported on this platform");
        return false;
#endif
    }

    private static int? TryParseFrameFromOutput(string line, int totalFrames)
    {
        var m = BracketedFrameRegex.Match(line);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var current) && int.TryParse(m.Groups[2].Value, out var total))
            return current;

        m = FrameLineRegex.Match(line);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var f))
            return f;

        m = SavedFrameRegex.Match(line);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var frame))
            return frame;

        return null;
    }

    private static void Report(IProgress<RenderProgressReport>? progress, string message, int? currentFrame = null, int? totalFrames = null, double? percentage = null)
    {
        progress?.Report(new RenderProgressReport
        {
            Message = message,
            CurrentFrame = currentFrame,
            TotalFrames = totalFrames,
            Percentage = percentage
        });
    }

    private string BuildBlenderArguments(BlenderFile blenderFile, BlenderRenderSettings? settings)
    {
        var args = $"-b \"{blenderFile.FilePath}\"";

        if (settings != null)
        {
            // Use Blender file's own output path and frame range - do NOT pass -o
            if (settings.IsAnimation && blenderFile.RenderType == RenderType.Animation)
                args += " -a";
            else
                args += $" -f {settings.FrameStart}";
        }
        else
        {
            var inputDir = Path.GetDirectoryName(blenderFile.FilePath) ?? "";
            var outputName = Path.GetFileNameWithoutExtension(blenderFile.FileName) + "_render";
            var ext = blenderFile.RenderType == RenderType.Image ? ".png" : ".mp4";
            var outputPath = Path.Combine(inputDir, outputName + (blenderFile.RenderType == RenderType.Animation ? "####" : "") + ext);
            args += $" -o \"{outputPath}\"";

            if (blenderFile.RenderType == RenderType.Image)
                args += " -f 1";
            else
                args += " -a";
        }

        return args;
    }

    private string ResolveOutputPath(BlenderFile blenderFile, BlenderRenderSettings? settings, string workingDir)
    {
        if (settings != null && !string.IsNullOrEmpty(settings.OutputPath))
        {
            var path = settings.OutputPath;
            if (!Path.IsPathRooted(path))
                path = Path.Combine(workingDir, path);
            if (settings.IsAnimation)
            {
                var dir = Path.GetDirectoryName(path) ?? "";
                var name = Path.GetFileNameWithoutExtension(path);
                var ext = settings.FileExtension;
                return Path.Combine(dir, $"{name}0001{ext}");
            }
            return path;
        }

        var inputDir = Path.GetDirectoryName(blenderFile.FilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var fileName = Path.GetFileNameWithoutExtension(blenderFile.FileName);
        var extension = blenderFile.RenderType == RenderType.Image ? ".png" : ".mp4";
        return Path.Combine(inputDir, $"{fileName}_render{extension}");
    }

    public string? GetBlenderExecutablePath()
    {
#if WINDOWS || MACCATALYST
        var exeName =
#if WINDOWS
            "blender.exe";
#else
            "blender";
#endif

        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var paths = pathEnv.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, exeName);
                if (File.Exists(fullPath))
                    return fullPath;
            }
        }

#if WINDOWS
        foreach (var pattern in _commonBlenderPaths)
        {
            var directory = Path.GetDirectoryName(pattern);
            if (directory == null) continue;
            var parentDir = Path.GetDirectoryName(directory);
            if (parentDir == null || !Directory.Exists(parentDir)) continue;

            var blenderDirs = Directory.GetDirectories(parentDir, "Blender *");
            foreach (var blenderDir in blenderDirs)
            {
                var exePath = Path.Combine(blenderDir, BlenderExecutableName);
                if (File.Exists(exePath))
                    return exePath;
            }
        }

        return null;
#else
        return null;
#endif
#else
        return null;
#endif
    }

    public bool IsBlenderInstalled()
    {
        return !string.IsNullOrEmpty(GetBlenderExecutablePath());
    }
}
