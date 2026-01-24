using AutoShut.Models;
using AutoShut.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;
#if WINDOWS
using Microsoft.UI.Xaml;
using WinUIDragEventArgs = Microsoft.UI.Xaml.DragEventArgs;
using WinUIDataPackageOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#endif

namespace AutoShut;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        Loaded += (s, e) =>
        {
#if WINDOWS
            SetupDragAndDrop();
#endif
        };
    }

    private void OnRemoveFileClicked(object? sender, EventArgs e)
    {
        if (sender is View view && view.BindingContext is BlenderFile file && !_viewModel.IsProcessing)
        {
            _viewModel.RemoveFileCommand.Execute(file);
        }
    }

    private void OnRenderTypePickerChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && picker.BindingContext is BlenderFile file && !_viewModel.IsProcessing)
        {
            if (picker.SelectedIndex is >= 0 and <= 1)
            {
                file.RenderType = picker.SelectedIndex == 0 ? RenderType.Image : RenderType.Animation;
            }
        }
    }

    private void OnRetryFileClicked(object? sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is BlenderFile file && !_viewModel.IsProcessing)
        {
            _viewModel.RetryFileCommand.Execute(file);
        }
    }

#if WINDOWS
    private void SetupDragAndDrop()
    {
        HandlerChanged += (s, e) =>
        {
            if (Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement element)
            {
                element.AllowDrop = true;
                element.DragOver += OnDragOver;
                element.Drop += OnDrop;
            }
        };

        if (Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement element2)
        {
            element2.AllowDrop = true;
            element2.DragOver += OnDragOver;
            element2.Drop += OnDrop;
        }
    }

    private void OnDragOver(object? sender, WinUIDragEventArgs e)
    {
        e.AcceptedOperation = WinUIDataPackageOperation.Copy;
        e.DragUIOverride.Caption = "Drop Blender files here";
    }

    private async void OnDrop(object? sender, WinUIDragEventArgs e)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems))
            return;

        try
        {
            var items = await e.DataView.GetStorageItemsAsync();
            var filePaths = items
                .OfType<StorageFile>()
                .Where(file => file.Path.EndsWith(".blend", StringComparison.OrdinalIgnoreCase) ||
                               file.Path.EndsWith(".blend1", StringComparison.OrdinalIgnoreCase))
                .Select(file => file.Path)
                .ToList();

            if (filePaths.Count > 0)
            {
                MainThread.BeginInvokeOnMainThread(() => _viewModel.AddFiles(filePaths));
            }
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
                await DisplayAlertAsync("Error", $"Unable to process files: {ex.Message}", "OK"));
        }
    }
#endif

    private async void OnBrowseFilesClicked(object? sender, EventArgs e)
    {
        try
        {
            var customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".blend", ".blend1" } },
                    { DevicePlatform.macOS, new[] { ".blend", ".blend1" } },
                    { DevicePlatform.Android, new[] { "application/x-blender" } },
                    { DevicePlatform.iOS, new[] { "com.blender.blend" } }
                });

            var options = new PickOptions
            {
                PickerTitle = "Select Blender files",
                FileTypes = customFileType
            };

            var results = await FilePicker.Default.PickMultipleAsync(options);
            if (results?.Any() == true)
            {
                var filePaths = results
                    .Where(f => f != null && !string.IsNullOrEmpty(f.FullPath))
                    .Select(f => f!.FullPath)
                    .ToList();
                
                _viewModel.AddFiles(filePaths);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Error", $"Unable to select files: {ex.Message}", "OK");
        }
    }
}
