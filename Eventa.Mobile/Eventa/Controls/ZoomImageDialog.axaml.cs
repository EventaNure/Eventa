using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Eventa.Views.Main;
using System;

namespace Eventa.Controls;

public partial class ZoomImageDialog : UserControl
{
    private static ZoomImageDialog? _instance;
    private double _currentZoom = 1.0;
    private const double ZoomStep = 0.25;
    private const double MinZoom = 0.5;
    private const double MaxZoom = 5.0;
    private const double WheelZoomFactor = 0.1;

    private bool _isPanning = false;
    private Point _lastPanPosition;
    private double _translateX = 0;
    private double _translateY = 0;

    private double _imageWidth = 0;
    private double _imageHeight = 0;

    public static ZoomImageDialog Instance
    {
        get
        {
            _instance ??= new ZoomImageDialog();
            return _instance;
        }
    }

    public ZoomImageDialog()
    {
        InitializeComponent();
    }

    public void Show(string imageUrl)
    {
        MainView.Instance.mainViewModel.ShowImageDialog();

        ZoomImage.Source = imageUrl;

        // Wait for image to load to get dimensions
        ZoomImage.PropertyChanged += (s, e) =>
        {
            if (e.Property.Name == nameof(ZoomImage.Bounds))
            {
                _imageWidth = ZoomImage.Bounds.Width;
                _imageHeight = ZoomImage.Bounds.Height;
                CenterImage();
            }
        };

        _currentZoom = 1.0;
        _translateX = 0;
        _translateY = 0;
        _isPanning = false;

        // Show the dialog
        DialogOverlay.IsVisible = true;

        // Center image after a short delay to ensure container is ready
        Avalonia.Threading.Dispatcher.UIThread.Post(() => CenterImage(), Avalonia.Threading.DispatcherPriority.Loaded);
    }

    private void OnClose(object? sender, RoutedEventArgs e)
    {
        DialogOverlay.IsVisible = false;
    }

    private void OnZoomIn(object? sender, RoutedEventArgs e)
    {
        if (_currentZoom < MaxZoom)
        {
            ZoomAtCenter(_currentZoom + ZoomStep);
        }
    }

    private void OnZoomOut(object? sender, RoutedEventArgs e)
    {
        if (_currentZoom > MinZoom)
        {
            ZoomAtCenter(_currentZoom - ZoomStep);
        }
    }

    private void OnResetZoom(object? sender, RoutedEventArgs e)
    {
        _currentZoom = 1.0;
        CenterImage();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(CanvasContainer).Properties.IsLeftButtonPressed)
        {
            _isPanning = true;
            _lastPanPosition = e.GetPosition(CanvasContainer);
            e.Pointer.Capture(CanvasContainer);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isPanning)
        {
            var currentPosition = e.GetPosition(CanvasContainer);
            var deltaX = currentPosition.X - _lastPanPosition.X;
            var deltaY = currentPosition.Y - _lastPanPosition.Y;

            _translateX += deltaX;
            _translateY += deltaY;

            _lastPanPosition = currentPosition;
            UpdateTransform();
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isPanning)
        {
            _isPanning = false;
            e.Pointer.Capture(null);
        }
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        var delta = e.Delta.Y;
        var newZoom = _currentZoom + (delta * WheelZoomFactor);
        newZoom = Math.Clamp(newZoom, MinZoom, MaxZoom);

        if (Math.Abs(newZoom - _currentZoom) > 0.01)
        {
            // Zoom towards mouse position
            var mousePos = e.GetPosition(CanvasContainer);
            ZoomAtPoint(newZoom, mousePos);
        }

        e.Handled = true;
    }

    private void ZoomAtCenter(double newZoom)
    {
        if (CanvasContainer.Bounds.Width > 0 && CanvasContainer.Bounds.Height > 0)
        {
            var centerPoint = new Point(
                CanvasContainer.Bounds.Width / 2,
                CanvasContainer.Bounds.Height / 2
            );
            ZoomAtPoint(newZoom, centerPoint);
        }
    }

    private void ZoomAtPoint(double newZoom, Point zoomPoint)
    {
        // Calculate the position in the image space before zoom
        var imageX = (zoomPoint.X - _translateX) / _currentZoom;
        var imageY = (zoomPoint.Y - _translateY) / _currentZoom;

        // Update zoom
        _currentZoom = newZoom;

        // Calculate new translation to keep the zoom point stable
        _translateX = zoomPoint.X - (imageX * _currentZoom);
        _translateY = zoomPoint.Y - (imageY * _currentZoom);

        UpdateTransform();
    }

    private void CenterImage()
    {
        if (CanvasContainer.Bounds.Width > 0 && CanvasContainer.Bounds.Height > 0 &&
            _imageWidth > 0 && _imageHeight > 0)
        {
            var containerWidth = CanvasContainer.Bounds.Width;
            var containerHeight = CanvasContainer.Bounds.Height;

            var scaledWidth = _imageWidth * _currentZoom;
            var scaledHeight = _imageHeight * _currentZoom;

            _translateX = (containerWidth - scaledWidth) / 2;
            _translateY = (containerHeight - scaledHeight) / 2;

            UpdateTransform();
        }
    }

    private void UpdateTransform()
    {
        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(new ScaleTransform(_currentZoom, _currentZoom));
        transformGroup.Children.Add(new TranslateTransform(_translateX, _translateY));

        ImageCanvas.RenderTransform = transformGroup;
    }
}