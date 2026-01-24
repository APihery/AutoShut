using AutoShut.Models;
using System.Diagnostics;
using System.Text;

namespace AutoShut.Services;

public class BlenderRenderer : IBlenderRenderer
{
    private const string BlenderExecutableName = "blender.exe";
    private readonly string[] _commonBlenderPaths = new[]
    {
        @"C:\Program Files\Blender Foundation\Blender *\blender.exe",
        @"C:\Program Files (x86)\Blender Foundation\Blender *\blender.exe",
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\Programs\Blender Foundation\Blender *\blender.exe"
    };

    public async Task<bool> RenderAsync(BlenderFile blenderFile, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
#if WINDOWS || MACCATALYST
        try
        {
            var blenderPath = GetBlenderExecutablePath();
            if (string.IsNullOrEmpty(blenderPath))
            {
                progress?.Report("Blender is not installed or not found");
                return false;
            }

            if (!File.Exists(blenderFile.FilePath))
            {
                progress?.Report($"File {blenderFile.FileName} does not exist");
                return false;
            }

            progress?.Report($"Starting render of {blenderFile.FileName}...");

            var outputPath = GetOutputPath(blenderFile);
            var arguments = BuildBlenderArguments(blenderFile, outputPath);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = blenderPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processStartInfo };
            
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                outputBuilder.AppendLine(e.Data);
                progress?.Report(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;
                errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                blenderFile.OutputPath = outputPath;
                progress?.Report($"Render completed successfully: {blenderFile.FileName}");
                return true;
            }

            blenderFile.ErrorMessage = errorBuilder.ToString();
            progress?.Report($"Error during render: {blenderFile.FileName}");
            return false;
        }
        catch (Exception ex)
        {
            blenderFile.ErrorMessage = ex.Message;
            progress?.Report($"Exception: {ex.Message}");
            return false;
        }
#else
        await Task.CompletedTask;
        progress?.Report("Blender rendering not supported on this platform");
        return false;
#endif
    }

    private string BuildBlenderArguments(BlenderFile blenderFile, string outputPath)
    {
        var args = $"-b \"{blenderFile.FilePath}\"";
        var outputDir = Path.GetDirectoryName(outputPath) ?? "";
        var outputName = Path.GetFileNameWithoutExtension(outputPath);

        if (blenderFile.RenderType == RenderType.Image)
        {
            args += $" -o \"{Path.Combine(outputDir, outputName)}\"";
            args += " -f 1";
        }
        else
        {
            args += $" -o \"{Path.Combine(outputDir, outputName)}####\"";
            args += " -a";
        }

        return args;
    }

    private string GetOutputPath(BlenderFile blenderFile)
    {
        var inputDir = Path.GetDirectoryName(blenderFile.FilePath) ?? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var fileName = Path.GetFileNameWithoutExtension(blenderFile.FileName);
        var extension = blenderFile.RenderType == RenderType.Image ? ".png" : ".mp4";
        return Path.Combine(inputDir, $"{fileName}_render{extension}");
    }

    public string? GetBlenderExecutablePath()
    {
        var pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(pathEnv))
        {
            var paths = pathEnv.Split(Path.PathSeparator);
            foreach (var path in paths)
            {
                var fullPath = Path.Combine(path, BlenderExecutableName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
        }

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
                {
                    return exePath;
                }
            }
        }

        return null;
    }

    public bool IsBlenderInstalled()
    {
        return !string.IsNullOrEmpty(GetBlenderExecutablePath());
    }
}
