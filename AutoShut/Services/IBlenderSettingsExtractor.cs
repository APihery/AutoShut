using AutoShut.Models;

namespace AutoShut.Services;

public interface IBlenderSettingsExtractor
{
    Task<BlenderRenderSettings?> ExtractAsync(string blendFilePath, string blenderExecutablePath, CancellationToken cancellationToken = default);
}
