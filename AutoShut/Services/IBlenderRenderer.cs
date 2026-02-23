using AutoShut.Models;

namespace AutoShut.Services;

public interface IBlenderRenderer
{
    Task<bool> RenderAsync(BlenderFile blenderFile, IProgress<RenderProgressReport>? progress = null, CancellationToken cancellationToken = default);
    string? GetBlenderExecutablePath();
    bool IsBlenderInstalled();
}
