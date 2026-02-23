using AutoShut.Models;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AutoShut.Services;

public class BlenderSettingsExtractor : IBlenderSettingsExtractor
{

    private const string ExtractScript = """
        import bpy
        import json
        import os

        try:
            scene = bpy.context.scene
            render = scene.render
            # Resolve // to absolute path
            filepath = bpy.path.abspath(render.filepath)
            frame_start = scene.frame_start
            frame_end = scene.frame_end
            ext = render.file_extension or ".png"
            if not ext.startswith("."):
                ext = "." + ext
            result = {
                "output_path": filepath,
                "frame_start": frame_start,
                "frame_end": frame_end,
                "file_extension": ext
            }
            print("AUTOSHUT_SETTINGS:" + json.dumps(result))
        except Exception as e:
            print("AUTOSHUT_ERROR:" + str(e))
        """;

    public async Task<BlenderRenderSettings?> ExtractAsync(string blendFilePath, string blenderExecutablePath, CancellationToken cancellationToken = default)
    {
#if WINDOWS || MACCATALYST
        var blenderPath = blenderExecutablePath;
        if (string.IsNullOrEmpty(blenderPath) || !File.Exists(blenderPath) || !File.Exists(blendFilePath))
            return null;

        var scriptPath = Path.Combine(Path.GetTempPath(), $"autoshut_extract_{Guid.NewGuid():N}.py");
        try
        {
            await File.WriteAllTextAsync(scriptPath, ExtractScript, cancellationToken);

            var startInfo = new ProcessStartInfo
            {
                FileName = blenderPath,
                Arguments = $"-b \"{blendFilePath}\" --background -P \"{scriptPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Path.GetDirectoryName(blendFilePath) ?? ""
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var outputText = await outputTask;
            var errorText = await errorTask;
            var text = outputText + "\n" + errorText;
            var match = Regex.Match(text, @"AUTOSHUT_SETTINGS:(.+)");
            if (match.Success)
            {
                var json = match.Groups[1].Value.Trim();
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                return new BlenderRenderSettings
                {
                    OutputPath = root.GetProperty("output_path").GetString() ?? "",
                    FrameStart = root.GetProperty("frame_start").GetInt32(),
                    FrameEnd = root.GetProperty("frame_end").GetInt32(),
                    FileExtension = root.GetProperty("file_extension").GetString() ?? ".png"
                };
            }
        }
        catch
        {
            // Fallback: return null, caller will use defaults
        }
        finally
        {
            try { if (File.Exists(scriptPath)) File.Delete(scriptPath); } catch { }
        }
#endif
        return null;
    }
}
