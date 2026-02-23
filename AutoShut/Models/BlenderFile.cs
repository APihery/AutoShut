using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoShut.Models;

public class BlenderFile : INotifyPropertyChanged
{
    private string _filePath = string.Empty;
    private RenderType _renderType = RenderType.Image;
    private RenderStatus _status = RenderStatus.Pending;
    private string? _outputPath;
    private string? _errorMessage;
    private DateTime? _startTime;
    private DateTime? _endTime;
    private int _currentFrame;
    private int _totalFrames;
    private double _renderProgressPercentage;
    private string? _expectedOutputPath;

    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath == value) return;
            _filePath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(FileName));
        }
    }

    public string FileName => Path.GetFileName(FilePath);

    public RenderType RenderType
    {
        get => _renderType;
        set
        {
            if (_renderType == value) return;
            _renderType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowFrameProgress));
            OnPropertyChanged(nameof(ShowProgressBar));
            OnPropertyChanged(nameof(ProgressDescription));
        }
    }

    public RenderStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowProgressBar));
        }
    }

    public string? OutputPath
    {
        get => _outputPath;
        set
        {
            if (_outputPath == value) return;
            _outputPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayOutputPath));
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (_errorMessage == value) return;
            _errorMessage = value;
            OnPropertyChanged();
        }
    }

    public DateTime? StartTime
    {
        get => _startTime;
        set
        {
            if (_startTime == value) return;
            _startTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Duration));
        }
    }

    public DateTime? EndTime
    {
        get => _endTime;
        set
        {
            if (_endTime == value) return;
            _endTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Duration));
        }
    }

    public TimeSpan? Duration => EndTime.HasValue && StartTime.HasValue 
        ? EndTime.Value - StartTime.Value 
        : null;

    public int CurrentFrame
    {
        get => _currentFrame;
        set
        {
            if (_currentFrame == value) return;
            _currentFrame = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressDescription));
        }
    }

    public int TotalFrames
    {
        get => _totalFrames;
        set
        {
            if (_totalFrames == value) return;
            _totalFrames = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ShowFrameProgress));
            OnPropertyChanged(nameof(ShowProgressBar));
            OnPropertyChanged(nameof(ProgressDescription));
        }
    }

    public double RenderProgressPercentage
    {
        get => _renderProgressPercentage;
        set
        {
            if (Math.Abs(_renderProgressPercentage - value) < 0.01) return;
            _renderProgressPercentage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ProgressDescription));
        }
    }

    public bool ShowFrameProgress => RenderType == RenderType.Animation && TotalFrames > 1;

    public bool ShowProgressBar => Status == RenderStatus.Processing && ShowFrameProgress;

    public string ProgressDescription => ShowFrameProgress
        ? $"Frame {CurrentFrame}/{TotalFrames} ({RenderProgressPercentage:F0}%)"
        : Status == RenderStatus.Processing ? "Rendering..." : "";

    public string? ExpectedOutputPath
    {
        get => _expectedOutputPath;
        set
        {
            if (_expectedOutputPath == value) return;
            _expectedOutputPath = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayOutputPath));
        }
    }

    public string? DisplayOutputPath => OutputPath ?? ExpectedOutputPath;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public enum RenderType
{
    Image,
    Animation
}

public enum RenderStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}
