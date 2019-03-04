using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Eyedropper.UWP
{
    public partial class Eyedropper : Control
    {
        public event TypedEventHandler<Eyedropper, ColorChangedEventArgs> ColorChanged;
        public event TypedEventHandler<Eyedropper, EventArgs> PickEnded;
        public event TypedEventHandler<Eyedropper, EventArgs> PickStarted;

        private readonly Popup _popup;
        private readonly Grid _rootGrid;
        private readonly Grid _targetGrid;
        private static readonly CoreCursor _defaultCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        private static readonly CoreCursor _moveCursor = new CoreCursor(CoreCursorType.Cross, 1);
        private TaskCompletionSource<Color> _taskSource;
        private uint _pointerId;
        private readonly TranslateTransform _layoutTransform = new TranslateTransform();
        private CanvasBitmap _appScreenshot;
        private readonly CanvasDevice _device = CanvasDevice.GetSharedDevice();
        private const int PreviewPixelsPerRawPixel = 10;
        private const int PixelCountPerRow = 11;
        private readonly CanvasImageSource _previewImageSource;
        private Action _lazyTask = null;

        public Eyedropper()
        {
            this.DefaultStyleKey = typeof(Eyedropper);
            _rootGrid = new Grid();
            _targetGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(0x01, 0x00, 0x00, 0x00))
            };
            _popup = new Popup
            {
                Child = _rootGrid
            };
            RenderTransform = _layoutTransform;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            IsHitTestVisible = false;
            _previewImageSource = new CanvasImageSource(_device, PreviewPixelsPerRawPixel * PixelCountPerRow, PreviewPixelsPerRawPixel * PixelCountPerRow, 96f);
            Preview = _previewImageSource;
            this.Loaded += Eyedropper_Loaded;
        }


        public async Task<Color> Open(Point? startPoint = null)
        {
            _taskSource = new TaskCompletionSource<Color>();
            HookUpEvents();
            this.Opacity = 0;
            if (startPoint.HasValue)
            {
                _lazyTask = async () =>
                {
                    await UpdateAppScreenshotAsync();
                    UpdateEyedropper(startPoint.Value);
                    this.Opacity = 1;
                };
            }
            _rootGrid.Children.Add(_targetGrid);
            _rootGrid.Children.Add(this);
            _rootGrid.Width= Window.Current.Bounds.Width;
            _rootGrid.Height = Window.Current.Bounds.Height;
            this.UpadateWorkArea();
            _popup.IsOpen = true;
            var result = await _taskSource.Task;
            _taskSource = null;
            _targetGrid.Children.Clear();
            _rootGrid.Children.Clear();
            return result;
        }

        public void Close()
        {
            if (_taskSource != null && !_taskSource.Task.IsCanceled)
            {
                _taskSource.SetCanceled();
                _rootGrid.Children.Clear();
            }
        }

        private void HookUpEvents()
        {
            Unloaded += Eyedropper_Unloaded;
            Window.Current.SizeChanged += Window_SizeChanged;
            DisplayInformation.GetForCurrentView().DpiChanged += Eyedropper_DpiChanged;
            _targetGrid.PointerEntered += TargetGrid_PointerEntered;
            _targetGrid.PointerExited += TargetGrid_PointerExited;
            _targetGrid.PointerPressed += TargetGrid_PointerPressed;
            _targetGrid.PointerMoved += TargetGrid_PointerMoved;
            _targetGrid.PointerReleased += TargetGrid_PointerReleased;
        }

        private void UnhookEvents()
        {
            Unloaded -= Eyedropper_Unloaded;
            Window.Current.SizeChanged -= Window_SizeChanged;
            DisplayInformation.GetForCurrentView().DpiChanged -= Eyedropper_DpiChanged;
            if (_targetGrid != null)
            {
                _targetGrid.PointerEntered -= TargetGrid_PointerEntered;
                _targetGrid.PointerExited -= TargetGrid_PointerExited;
                _targetGrid.PointerPressed -= TargetGrid_PointerPressed;
                _targetGrid.PointerMoved -= TargetGrid_PointerMoved;
                _targetGrid.PointerReleased -= TargetGrid_PointerReleased;
            }
        }

        private void Eyedropper_Loaded(object sender, RoutedEventArgs e)
        {
            _lazyTask?.Invoke();
            _lazyTask = null;
        }

        private void TargetGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = _defaultCursor;
        }

        private void TargetGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = _moveCursor;
        }

        private async void Eyedropper_DpiChanged(DisplayInformation sender, object args)
        {
            await UpdateAppScreenshotAsync();
        }

        private void TargetGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.Pointer;
            if (pointer.PointerId == _pointerId)
            {
                var point = e.GetCurrentPoint(_rootGrid);
                UpdateEyedropper(point.Position);
                PickEnded?.Invoke(this, EventArgs.Empty);
                _pointerId = 0;
                if (!_taskSource.Task.IsCanceled)
                    _taskSource.SetResult(Color);
            }
        }

        private void TargetGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.Pointer;
            if (pointer.PointerId == _pointerId)
            {
                var point = e.GetCurrentPoint(_rootGrid);
                UpdateEyedropper(point.Position);
            }
        }

        private async void TargetGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _pointerId = e.Pointer.PointerId;
            PickStarted?.Invoke(this, EventArgs.Empty);
            await UpdateAppScreenshotAsync();
            var point = e.GetCurrentPoint(_rootGrid);
            UpdateEyedropper(point.Position);

            if (this.Opacity < 1)
            {
                this.Opacity = 1;
            }
        }

        private void Eyedropper_Unloaded(object sender, RoutedEventArgs e)
        {
            UnhookEvents();
            if (_popup != null)
            {
                _popup.IsOpen = false;
            }

            _appScreenshot?.Dispose();
            _appScreenshot = null;

            Window.Current.CoreWindow.PointerCursor = _defaultCursor;
        }

        private async void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            if (_rootGrid != null)
            {
                _rootGrid.Width = Window.Current.Bounds.Width;
                _rootGrid.Height = Window.Current.Bounds.Height;
            }

            await UpdateAppScreenshotAsync();
        }

    }
}
