using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Eyedropper.UWP
{
    public partial class EyedropperToolButton
    {
        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(EyedropperToolButton),
                new PropertyMetadata(default(Color)));

        // Using a DependencyProperty as the backing store for EyedropperEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EyedropperEnabledProperty =
            DependencyProperty.Register(nameof(EyedropperEnabled), typeof(bool), typeof(EyedropperToolButton),
                new PropertyMetadata(false, OnEyedropperEnabledChanged));

        // Using a DependencyProperty as the backing store for EyedropperStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EyedropperStyleProperty =
            DependencyProperty.Register(nameof(EyedropperStyle), typeof(Style), typeof(EyedropperToolButton),
                new PropertyMetadata(default(Style), OnEyedropperStyleChanged));

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(EyedropperToolButton),
                new PropertyMetadata(default(FrameworkElement), OnTargetChanged));

        public Color Color
        {
            get => (Color) GetValue(ColorProperty);
            private set => SetValue(ColorProperty, value);
        }

        public bool EyedropperEnabled
        {
            get => (bool) GetValue(EyedropperEnabledProperty);
            set => SetValue(EyedropperEnabledProperty, value);
        }


        public Style EyedropperStyle
        {
            get => (Style) GetValue(EyedropperStyleProperty);
            set => SetValue(EyedropperStyleProperty, value);
        }

        public FrameworkElement Target
        {
            get => (FrameworkElement) GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        private static void OnEyedropperEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EyedropperToolButton eyedropperToolButton)
            {
                if (eyedropperToolButton.EyedropperEnabled)
                {
                    VisualStateManager.GoToState(eyedropperToolButton,
                        eyedropperToolButton.IsPointerOver ? EyedropperEnabledPointerOverState : EyedropperEnabledState,
                        true);
                    eyedropperToolButton._eyedropper.Open().ConfigureAwait(false);
                }
                else
                {
                    VisualStateManager.GoToState(eyedropperToolButton,
                        eyedropperToolButton.IsPointerOver ? PointerOverState : NormalState, true);
                    eyedropperToolButton._eyedropper.Close();
                }
            }
        }

        private static void OnEyedropperStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EyedropperToolButton eyedropperToolButton)
                eyedropperToolButton._eyedropper.Style = eyedropperToolButton.EyedropperStyle;
        }

        private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EyedropperToolButton eyedropperToolButton)
            {
                eyedropperToolButton.UnhookTargetEvents(e.OldValue as FrameworkElement);
                eyedropperToolButton.HookUpTargetEvents(e.NewValue as FrameworkElement);
            }
        }

        private void UnhookTargetEvents(FrameworkElement target)
        {
            if (target != null)
            {
                target.SizeChanged -= Target_SizeChanged;
                target.PointerEntered -= Target_PointerEntered;
            }
        }

        private void HookUpTargetEvents(FrameworkElement target)
        {
            if (target != null)
            {
                target.SizeChanged += Target_SizeChanged;
                target.PointerEntered += Target_PointerEntered;
            }
        }

        private void Target_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            UpadateEyedropperWorkArea();
        }

        private void Target_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpadateEyedropperWorkArea();
        }
    }
}