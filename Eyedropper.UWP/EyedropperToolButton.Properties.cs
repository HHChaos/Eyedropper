using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Eyedropper.UWP
{
    public partial class EyedropperToolButton
    {
        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Color.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(EyedropperToolButton),
                new PropertyMetadata(default(Color)));

        public bool EyedropperEnabled
        {
            get { return (bool)GetValue(EyedropperEnabledProperty); }
            set { SetValue(EyedropperEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EyedropperEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EyedropperEnabledProperty =
            DependencyProperty.Register("EyedropperEnabled", typeof(bool), typeof(EyedropperToolButton), new PropertyMetadata(false, OnEyedropperEnabledChanged));

        public static void OnEyedropperEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EyedropperToolButton eyedropperToolButton)
            {
                if (eyedropperToolButton.EyedropperEnabled)
                {
                    VisualStateManager.GoToState(eyedropperToolButton, eyedropperToolButton.IsPointerOver ? EyedropperEnabledPointerOverState : EyedropperEnabledState, true);
                    eyedropperToolButton.eyedropper.Open().ConfigureAwait(false);
                }
                else
                {
                    VisualStateManager.GoToState(eyedropperToolButton, eyedropperToolButton.IsPointerOver ? PointerOverState : NormalState, true);
                    eyedropperToolButton.eyedropper.Close();
                }
            }
        }

        public FrameworkElement Target
        {
            get { return (FrameworkElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Target.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(EyedropperToolButton), new PropertyMetadata(default(FrameworkElement), OnTargetChanged));

        public static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
