using Microsoft.Maui.Controls;

namespace AutoShut.Behaviors;

public class HoverImageButtonBehavior : Behavior<ImageButton>
{
    private ImageButton? _imageButton;
    private bool _isHovered;
    
    private static readonly Color ListItemColor = Color.FromArgb("#1F2937");
    private static readonly Color HoverColor = Color.FromArgb("#2D3748");

    protected override void OnAttachedTo(ImageButton bindable)
    {
        base.OnAttachedTo(bindable);
        _imageButton = bindable;
        bindable.HandlerChanged += OnHandlerChanged;
        bindable.Loaded += OnButtonLoaded;
        try
        {
            InitializeBackgroundColor();
            AttachEvents();
        }
        catch { }
    }

    protected override void OnDetachingFrom(ImageButton bindable)
    {
        bindable.HandlerChanged -= OnHandlerChanged;
        bindable.Loaded -= OnButtonLoaded;
        DetachEvents();
        _imageButton = null;
        base.OnDetachingFrom(bindable);
    }

    private void OnButtonLoaded(object? sender, EventArgs e)
    {
        try
        {
            InitializeBackgroundColor();
            AttachEvents();
        }
        catch { }
    }

    private void OnHandlerChanged(object? sender, EventArgs e)
    {
        DetachEvents();
        AttachEvents();
    }

    private void InitializeBackgroundColor()
    {
        if (_imageButton == null) return;
        var currentBg = _imageButton.BackgroundColor ?? Colors.Transparent;
        if (currentBg == Colors.Transparent || currentBg.Alpha < 0.01)
        {
            _imageButton.BackgroundColor = ListItemColor;
        }
    }

    private void AttachEvents()
    {
#if WINDOWS
        if (_imageButton?.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button platformButton)
        {
            try
            {
                platformButton.PointerEntered -= OnPointerEntered;
                platformButton.PointerExited -= OnPointerExited;
                platformButton.PointerEntered += OnPointerEntered;
                platformButton.PointerExited += OnPointerExited;
            }
            catch { }
        }
        else if (_imageButton != null)
        {
            Task.Delay(100).ContinueWith(_ =>
            {
                if (_imageButton != null && Microsoft.Maui.ApplicationModel.MainThread.IsMainThread)
                {
                    try
                    {
                        AttachEvents();
                    }
                    catch { }
                }
                else if (_imageButton != null)
                {
                    try
                    {
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (_imageButton != null)
                            {
                                AttachEvents();
                            }
                        });
                    }
                    catch { }
                }
            });
        }
#endif
    }

    private void DetachEvents()
    {
#if WINDOWS
        try
        {
            if (_imageButton?.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button platformButton)
            {
                platformButton.PointerEntered -= OnPointerEntered;
                platformButton.PointerExited -= OnPointerExited;
            }
        }
        catch { }
#endif
    }

#if WINDOWS
    private void OnPointerEntered(object? sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (_imageButton == null || _isHovered || !_imageButton.IsEnabled) return;
        _isHovered = true;
        var currentBg = _imageButton.BackgroundColor ?? Colors.Transparent;
        if (currentBg == Colors.Transparent || currentBg.Alpha < 0.01)
        {
            _imageButton.BackgroundColor = ListItemColor;
        }
        _imageButton.BackgroundColor = HoverColor;
    }

    private void OnPointerExited(object? sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (_imageButton == null || !_isHovered) return;
        _isHovered = false;
        _imageButton.BackgroundColor = ListItemColor;
    }
#endif
}
