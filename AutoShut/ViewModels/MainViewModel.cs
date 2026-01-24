using AutoShut.Models;
using AutoShut.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AutoShut.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly IBlenderRenderer _blenderRenderer;
    private bool _isProcessing;
    private int _completedCount;
    private int _totalCount;
    private string _statusMessage = "Ready";
    private bool _autoShutdown;
    private CancellationTokenSource? _cancellationTokenSource;

    public MainViewModel(IBlenderRenderer blenderRenderer)
    {
        _blenderRenderer = blenderRenderer;
        BlenderFiles = new ObservableCollection<BlenderFile>();
        BlenderFiles.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(HasFiles));
            OnPropertyChanged(nameof(BlenderFiles));
        };
        AddFileCommand = new Command<string>(OnAddFile);
        RemoveFileCommand = new Command<BlenderFile>(OnRemoveFile);
        RetryFileCommand = new Command<BlenderFile>(OnRetryFile);
        StartRenderCommand = new Command(OnStartRender, () => !IsProcessing && BlenderFiles.Count > 0);
        CancelRenderCommand = new Command(OnCancelRender, () => IsProcessing);
        ClearFilesCommand = new Command(OnClearFiles, () => !IsProcessing && BlenderFiles.Count > 0);
    }

    public ObservableCollection<BlenderFile> BlenderFiles { get; }
    
    public bool HasFiles => BlenderFiles.Count > 0;

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (_isProcessing == value) return;
            _isProcessing = value;
            OnPropertyChanged();
            StartRenderCommand.ChangeCanExecute();
            CancelRenderCommand.ChangeCanExecute();
            ClearFilesCommand.ChangeCanExecute();
        }
    }

    public int CompletedCount
    {
        get => _completedCount;
        set
        {
            if (_completedCount == value) return;
            _completedCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }

    public int TotalCount
    {
        get => _totalCount;
        set
        {
            if (_totalCount == value) return;
            _totalCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressPercentage));
        }
    }

    public double ProgressPercentage => TotalCount > 0 ? (double)CompletedCount / TotalCount * 100 : 0;

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage == value) return;
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool AutoShutdown
    {
        get => _autoShutdown;
        set
        {
            if (_autoShutdown == value) return;
            _autoShutdown = value;
            OnPropertyChanged();
        }
    }

    public Command<string> AddFileCommand { get; }
    public Command<BlenderFile> RemoveFileCommand { get; }
    public Command<BlenderFile> RetryFileCommand { get; }
    public Command StartRenderCommand { get; }
    public Command CancelRenderCommand { get; }
    public Command ClearFilesCommand { get; }

    public void AddFiles(IEnumerable<string> filePaths)
    {
        var existingPaths = new HashSet<string>(BlenderFiles.Select(f => f.FilePath), StringComparer.OrdinalIgnoreCase);
        var added = false;
        
        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath)) continue;
            if (!filePath.EndsWith(".blend", StringComparison.OrdinalIgnoreCase) &&
                !filePath.EndsWith(".blend1", StringComparison.OrdinalIgnoreCase)) continue;
            if (existingPaths.Contains(filePath)) continue;

            BlenderFiles.Add(new BlenderFile
            {
                FilePath = filePath,
                RenderType = RenderType.Image,
                Status = RenderStatus.Pending
            });
            existingPaths.Add(filePath);
            added = true;
        }
        
        if (!added) return;

        TotalCount = BlenderFiles.Count;
        OnPropertyChanged(nameof(HasFiles));
        OnPropertyChanged(nameof(BlenderFiles));
        StartRenderCommand.ChangeCanExecute();
        ClearFilesCommand.ChangeCanExecute();
    }

    private void OnAddFile(string filePath)
    {
        AddFiles(new[] { filePath });
    }

    private void OnRemoveFile(BlenderFile file)
    {
        if (IsProcessing || !BlenderFiles.Contains(file)) return;
        BlenderFiles.Remove(file);
        TotalCount = BlenderFiles.Count;
        StartRenderCommand.ChangeCanExecute();
        ClearFilesCommand.ChangeCanExecute();
    }

    private void OnRetryFile(BlenderFile file)
    {
        if (IsProcessing || file.Status != RenderStatus.Failed) return;
        file.Status = RenderStatus.Pending;
        file.ErrorMessage = null;
        file.StartTime = null;
        file.EndTime = null;
    }

    private void OnClearFiles()
    {
        if (IsProcessing) return;
        BlenderFiles.Clear();
        CompletedCount = 0;
        TotalCount = 0;
        StatusMessage = "Ready";
        StartRenderCommand.ChangeCanExecute();
        ClearFilesCommand.ChangeCanExecute();
    }

    private async void OnStartRender()
    {
        if (BlenderFiles.Count == 0 || IsProcessing) return;

        if (!_blenderRenderer.IsBlenderInstalled())
        {
            StatusMessage = "Blender is not installed or not found";
            return;
        }

        IsProcessing = true;
        CompletedCount = 0;
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        StatusMessage = "Starting renders...";

        try
        {
            var pendingFiles = BlenderFiles
                .Where(f => f.Status == RenderStatus.Pending)
                .GroupBy(f => f.FilePath, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            foreach (var file in pendingFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    file.Status = RenderStatus.Cancelled;
                    StatusMessage = "Renders cancelled";
                    break;
                }

                file.Status = RenderStatus.Processing;
                file.StartTime = DateTime.Now;
                StatusMessage = $"Rendering: {file.FileName}";

                var progress = new Progress<string>(_ => { });

                var success = await _blenderRenderer.RenderAsync(file, progress, cancellationToken);

                file.EndTime = DateTime.Now;
                file.Status = success ? RenderStatus.Completed : RenderStatus.Failed;

                CompletedCount++;
                StatusMessage = $"Completed: {CompletedCount}/{TotalCount}";
            }

            if (!cancellationToken.IsCancellationRequested)
            {
                var successCount = BlenderFiles.Count(f => f.Status == RenderStatus.Completed);
                StatusMessage = $"All renders completed ({successCount}/{TotalCount} successful)";
                
                if (AutoShutdown)
                {
                    await WaitForAllBlenderProcessesToComplete();
                    await Task.Delay(2000);
                    ShutdownComputer();
                }
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Renders cancelled";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private async Task WaitForAllBlenderProcessesToComplete()
    {
#if WINDOWS || MACCATALYST
        StatusMessage = "Verifying all Blender processes are closed...";
        
        const int maxWaitTime = 30000;
        const int checkInterval = 500;
        var elapsed = 0;

        while (elapsed < maxWaitTime)
        {
            var blenderProcesses = Process.GetProcesses()
                .Where(p => p.ProcessName.Equals("blender", StringComparison.OrdinalIgnoreCase) ||
                           p.ProcessName.Equals("blender.exe", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (blenderProcesses.Count == 0)
            {
                return;
            }

            var allExited = true;
            foreach (var process in blenderProcesses)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        allExited = false;
                        break;
                    }
                }
                catch
                {
                }
                finally
                {
                    try { process.Dispose(); } catch { }
                }
            }

            if (allExited)
            {
                return;
            }

            await Task.Delay(checkInterval);
            elapsed += checkInterval;
        }
#else
        await Task.CompletedTask;
#endif
    }

    private void ShutdownComputer()
    {
#if WINDOWS
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "shutdown.exe",
                Arguments = "/s /t 10",
                UseShellExecute = false,
                CreateNoWindow = true
            });
            StatusMessage = "Shutting down in 10 seconds...";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to shutdown: {ex.Message}";
        }
#elif MACCATALYST
        try
        {
            Process.Start("osascript", "-e 'tell app \"System Events\" to shut down'");
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to shutdown: {ex.Message}";
        }
#elif IOS
        StatusMessage = "Shutdown not supported on iOS";
#elif ANDROID
        StatusMessage = "Shutdown not supported on Android";
#endif
    }

    private void OnCancelRender()
    {
        _cancellationTokenSource?.Cancel();
        StatusMessage = "Cancelling...";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
