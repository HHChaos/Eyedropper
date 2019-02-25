using Microsoft.Graphics.Canvas;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas.UI.Xaml;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Eyedropper.UWP
{
    public sealed class Eyedropper : Control
    {
        public event TypedEventHandler<Eyedropper, Color> ColorChanged;
        public event TypedEventHandler<Eyedropper, EventArgs> PickEnded;
        public event TypedEventHandler<Eyedropper, EventArgs> PickStarted;

        private Popup _popup;
        private Grid _rootGrid;
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



        public Color Color
        {
            get { return (Color) GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(Eyedropper),
                new PropertyMetadata(default(Color)));



        public ImageSource Preview
        {
            get { return (ImageSource) GetValue(PreviewProperty); }
            set { SetValue(PreviewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Preview.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register(nameof(Preview), typeof(ImageSource), typeof(Eyedropper),
                new PropertyMetadata(default(ImageSource)));



        public Eyedropper()
        {
            this.DefaultStyleKey = typeof(Eyedropper);
            RenderTransform = _layoutTransform;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            IsHitTestVisible = false;
            _previewImageSource = new CanvasImageSource(_device, PreviewPixelsPerRawPixel* PixelCountPerRow, PreviewPixelsPerRawPixel * PixelCountPerRow, 96f);
            Preview = _previewImageSource;
            this.Loaded += Eyedropper_Loaded;
        }


        public async Task<Color> OpenEyedropper(Point? startPoint = null)
        {
            _taskSource = new TaskCompletionSource<Color>();
            if (_popup == null || _rootGrid == null)
            {
                _rootGrid = new Grid
                {
                    Width = Window.Current.Bounds.Width,
                    Height = Window.Current.Bounds.Height,
                    Background = new SolidColorBrush(Color.FromArgb(0x1, 0x00, 0x00, 0x00))
                };
                _popup = new Popup
                {
                    Child = _rootGrid
                };
            }

            HookUpEvents();
            this.Opacity = 0.01;
            if (startPoint.HasValue)
            {
                _lazyTask = async () =>
                {
                    await UpdateAppScreenshotAsync();
                    UpdateEyedropper(startPoint.Value);
                    this.Opacity = 1;
                };
            }

            _rootGrid.Children.Add(this);
            _popup.IsOpen = true;
            var result = await _taskSource.Task;
            _rootGrid.Children.Remove(this);
            return result;
        }

        private void UpdateEyedropper(Point position)
        {

            if (_appScreenshot == null)
            {
                return;
            }

            _layoutTransform.X = position.X - (this.ActualWidth / 2);
            _layoutTransform.Y = position.Y - (this.ActualHeight);

            var x = (int) Math.Ceiling(Math.Min(_appScreenshot.SizeInPixels.Width - 1, Math.Max(position.X, 0)));
            var y = (int) Math.Ceiling(Math.Min(_appScreenshot.SizeInPixels.Height - 1, Math.Max(position.Y, 0)));
            Color = _appScreenshot.GetPixelColors(x, y, 1, 1).Single();
            UpdatePreview(x, y);
        }

        private void UpdatePreview(int centerX, int centerY)
        {
            var halfPixelCountPerRow = (PixelCountPerRow - 1) / 2;
            var left = (int) Math.Min(_appScreenshot.SizeInPixels.Width - 1,
                Math.Max(centerX - halfPixelCountPerRow, 0));
            var top = (int) Math.Min(_appScreenshot.SizeInPixels.Height - 1,
                Math.Max(centerY - halfPixelCountPerRow, 0));
            var right = (int) Math.Min(centerX + halfPixelCountPerRow, _appScreenshot.SizeInPixels.Width - 1);
            var bottom = (int) Math.Min(centerY + halfPixelCountPerRow, _appScreenshot.SizeInPixels.Height - 1);
            var width = right - left + 1;
            var height = bottom - top + 1;
            var colors = _appScreenshot.GetPixelColors(left, top, width, height);


            var colorStartX = left - (centerX - halfPixelCountPerRow);
            var colorStartY = top - (centerY - halfPixelCountPerRow);
            var colorEndX = colorStartX + width;
            var colorEndY = colorStartY + height;
            
            var size = new Size(PreviewPixelsPerRawPixel, PreviewPixelsPerRawPixel);
            var startPoint = new Point(0, PreviewPixelsPerRawPixel * colorStartY);

            using (var drawingSession = _previewImageSource.CreateDrawingSession(Colors.White))
            {
                for (var i = colorStartY; i < colorEndY; i++)
                {
                    startPoint.X = colorStartX * PreviewPixelsPerRawPixel;
                    for (var j = colorStartX; j < colorEndX; j++)
                    {
                        var color = colors[(i - colorStartY) * width + (j - colorStartX)];
                        drawingSession.FillRectangle(new Rect(startPoint, size), color);
                        startPoint.X += PreviewPixelsPerRawPixel;
                    }

                    startPoint.Y += PreviewPixelsPerRawPixel;
                }
            }
        }

        private async Task UpdateAppScreenshotAsync()
        {
            var renderTarget = new RenderTargetBitmap();
            var diaplayInfo = DisplayInformation.GetForCurrentView();
            var scale = diaplayInfo.RawPixelsPerViewPixel;
            var scaleWidth = (int)Math.Ceiling(Window.Current.Bounds.Width / scale);
            var scaleHeight = (int)Math.Ceiling(Window.Current.Bounds.Height / scale);
            await renderTarget.RenderAsync(Window.Current.Content, scaleWidth, scaleHeight);
            var pixels = await renderTarget.GetPixelsAsync();
            _appScreenshot = CanvasBitmap.CreateFromBytes(_device, pixels, renderTarget.PixelWidth,
                renderTarget.PixelHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        }

        private void HookUpEvents()
        {
            Unloaded += Eyedropper_Unloaded;
            Window.Current.SizeChanged += Window_SizeChanged;
            DisplayInformation.GetForCurrentView().DpiChanged += Eyedropper_DpiChanged;
            _rootGrid.PointerEntered += _rootGrid_PointerEntered;
            _rootGrid.PointerExited += _rootGrid_PointerExited;
            _rootGrid.PointerPressed += _rootGrid_PointerPressed;
            _rootGrid.PointerMoved += _rootGrid_PointerMoved;
            _rootGrid.PointerReleased += _rootGrid_PointerReleased;
        }

        private void _rootGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = _defaultCursor;
        }

        private void _rootGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = _moveCursor;
        }

        private void Eyedropper_Loaded(object sender, RoutedEventArgs e)
        {
            _lazyTask?.Invoke();
            _lazyTask = null;
        }

        private async void Eyedropper_DpiChanged(DisplayInformation sender, object args)
        {
            await UpdateAppScreenshotAsync();
        }

        private void _rootGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.Pointer;
            if (pointer.PointerId == _pointerId)
            {
                var point = e.GetCurrentPoint(_rootGrid);
                UpdateEyedropper(point.Position);
                PickEnded?.Invoke(this, EventArgs.Empty);
                _pointerId = 0;
                _taskSource.SetResult(Color);
            }
        }

        private void _rootGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.Pointer;
            if (pointer.PointerId == _pointerId)
            {
                var point = e.GetCurrentPoint(_rootGrid);
                UpdateEyedropper(point.Position);
                ColorChanged?.Invoke(this, Color);
            }
        }

        private async void _rootGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _pointerId = e.Pointer.PointerId;
            PickStarted?.Invoke(this, EventArgs.Empty);
            if (_appScreenshot == null)
            {
                await UpdateAppScreenshotAsync();
            }

            var point = e.GetCurrentPoint(_rootGrid);
            UpdateEyedropper(point.Position);

            if (this.Opacity < 1)
            {
                this.Opacity = 1;
            }
        }

        private void UnhookEvents()
        {
            Unloaded -= Eyedropper_Unloaded;
            Window.Current.SizeChanged -= Window_SizeChanged;
            DisplayInformation.GetForCurrentView().DpiChanged -= Eyedropper_DpiChanged;
            if (_rootGrid != null)
            {
                _rootGrid.PointerEntered -= _rootGrid_PointerEntered;
                _rootGrid.PointerExited -= _rootGrid_PointerExited;
                _rootGrid.PointerPressed -= _rootGrid_PointerPressed;
                _rootGrid.PointerMoved -= _rootGrid_PointerMoved;
                _rootGrid.PointerReleased -= _rootGrid_PointerReleased;
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
