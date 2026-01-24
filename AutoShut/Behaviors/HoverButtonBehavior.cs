using Microsoft.Maui.Controls;

namespace AutoShut.Behaviors;

public class HoverButtonBehavior : Behavior<Button>
{
    private Button? _button;
    private bool _isHovered;
    private Color? _originalBackgroundColor;
    
    private static readonly Color ListItemColor = Color.FromArgb("#1F2937");
    private static readonly Color HoverColor = Color.FromArgb("#2D3748");

    protected override void OnAttachedTo(Button bindable)
    {
        base.OnAttachedTo(bindable);
        _button = bindable;
        bindable.HandlerChanged += OnHandlerChanged;
        bindable.Loaded += OnButtonLoaded;
        try
        {
            InitializeBackgroundColor();
            AttachEvents();
        }
        catch { }
    }

    protected override void OnDetachingFrom(Button bindable)
    {
        bindable.HandlerChanged -= OnHandlerChanged;
        bindable.Loaded -= OnButtonLoaded;
        DetachEvents();
        _button = null;
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
        if (_button == null) return;
        var currentBg = _button.BackgroundColor ?? Colors.Transparent;
        
        if (_originalBackgroundColor == null)
        {
            if (currentBg == Colors.Transparent || currentBg.Alpha < 0.01)
            {
                _originalBackgroundColor = ListItemColor;
                _button.BackgroundColor = ListItemColor;
            }
            else
            {
                _originalBackgroundColor = currentBg;
            }
        }
    }

    private void AttachEvents()
    {
#if WINDOWS
        if (_button?.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button platformButton)
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
        else if (_button != null)
        {
            Task.Delay(100).ContinueWith(_ =>
            {
                if (_button != null && Microsoft.Maui.ApplicationModel.MainThread.IsMainThread)
                {
                    try
                    {
                        AttachEvents();
                    }
                    catch { }
                }
                else if (_button != null)
                {
                    try
                    {
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                        {
                            if (_button != null)
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
            if (_button?.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button platformButton)
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
        if (_button == null || _isHovered || !_button.IsEnabled) return;
        _isHovered = true;
        
        if (_originalBackgroundColor == null)
        {
            var currentBg = _button.BackgroundColor ?? Colors.Transparent;
            if (currentBg == Colors.Transparent || currentBg.Alpha < 0.01)
            {
                _originalBackgroundColor = ListItemColor;
            }
            else
            {
                _originalBackgroundColor = currentBg;
            }
        }
        
        _button.BackgroundColor = HoverColor;
    }

    private void OnPointerExited(object? sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (_button == null || !_isHovered) return;
        _isHovered = false;
        
        if (_originalBackgroundColor != null)
        {
            _button.BackgroundColor = _originalBackgroundColor;
        }
        else
        {
            _button.BackgroundColor = ListItemColor;
        }
    }
#endif
}
