using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Eyedropper.UWP
{
    public partial class Eyedropper
    {
        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(Eyedropper),
                new PropertyMetadata(default(Color), OnColorChanged));

        // Using a DependencyProperty as the backing store for Preview.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PreviewProperty =
            DependencyProperty.Register(nameof(Preview), typeof(ImageSource), typeof(Eyedropper),
                new PropertyMetadata(default(ImageSource)));

        // Using a DependencyProperty as the backing store for WorkArea.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WorkAreaProperty =
            DependencyProperty.Register(nameof(WorkArea), typeof(Rect), typeof(Eyedropper),
                new PropertyMetadata(default(Rect), OnWorkAreaChanged));

        public Color Color
        {
            get => (Color) GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public ImageSource Preview
        {
            get => (ImageSource) GetValue(PreviewProperty);
            set => SetValue(PreviewProperty, value);
        }


        public Rect WorkArea
        {
            get => (Rect) GetValue(WorkAreaProperty);
            set => SetValue(WorkAreaProperty, value);
        }

        public static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Eyedropper eyedropper)
                eyedropper.ColorChanged?.Invoke(eyedropper,
                    new ColorChangedEventArgs {OldColor = (Color) e.OldValue, NewColor = (Color) e.NewValue});
        }

        public static void OnWorkAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Eyedropper eyedropper)
                eyedropper.UpadateWorkArea();
        }
    }
}