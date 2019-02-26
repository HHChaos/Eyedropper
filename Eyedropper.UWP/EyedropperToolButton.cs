using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace Eyedropper.UWP
{
    public class EyedropperToolButton:ToggleButton
    {
        private readonly Eyedropper eyedropper;

        public EyedropperToolButton()
        {
            this.RegisterPropertyChangedCallback(IsCheckedProperty, OnIsCheckedChanged);
            eyedropper = new Eyedropper();
            eyedropper.PickEnded += Eyedropper_PickEnded;
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

        private void HookUpTargetEvents(FrameworkElement target)
        {
            if (target != null)
            {
                target.SizeChanged += Target_SizeChanged;
                target.PointerEntered += Target_PointerEntered;
            }
        }

        private void Target_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpadateEyedropperWorkArea();
        }

        private void Target_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpadateEyedropperWorkArea();
        }

        private void UnhookTargetEvents(FrameworkElement target)
        {
            if (target != null)
            {
                target.SizeChanged -= Target_SizeChanged;
                target.PointerEntered -= Target_PointerEntered;
            }
        }

        private void UpadateEyedropperWorkArea()
        {
            if (Target != null)
            {
                var transform = Target.TransformToVisual(Window.Current.Content);
                var position = transform.TransformPoint(new Point());
                eyedropper.WorkArea = new Rect(position, new Size(Target.ActualWidth,Target.ActualHeight));
            }
        }

        private void Eyedropper_PickEnded(Eyedropper sender, EventArgs args)
        {
            IsChecked = false;
        }

        private void OnIsCheckedChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (IsChecked == true)
            {
                eyedropper.Open().ConfigureAwait(false);
            }
            else
            {
                eyedropper.Close();
            }
        }
        

    }
}
