using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Eyedropper.UWP
{
    public class EyedropperToolButton: ButtonBase
    {
        private const string NormalState = "Normal";
        private const string PointerOverState = "PointerOver";
        private const string PressedState = "Pressed";
        private const string DisabledState = "Disabled";
        private const string EyedropperEnabledState = "EyedropperEnabled";
        private const string EyedropperEnabledPointerOverState = "EyedropperEnabledPointerOver";
        private const string EyedropperEnabledPressedState = "EyedropperEnabledPressed";
        private const string EyedropperEnabledDisabledState = "EyedropperEnabledDisabled";

        /// <inheritdoc/>
        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            base.OnPointerEntered(e);

            VisualStateManager.GoToState(this, EyedropperEnabled ? EyedropperEnabledPointerOverState : PointerOverState, true);
        }

        /// <inheritdoc/>
        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            base.OnPointerExited(e);

            VisualStateManager.GoToState(this, EyedropperEnabled ? EyedropperEnabledState : NormalState, true);
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            base.OnPointerPressed(e);

            VisualStateManager.GoToState(this, EyedropperEnabled ? EyedropperEnabledState : NormalState, true);
        }

        private readonly Eyedropper eyedropper;

        public EyedropperToolButton()
        {
            this.DefaultStyleKey = typeof(EyedropperToolButton);
            this.RegisterPropertyChangedCallback(IsEnabledProperty, OnIsEnabledChanged);
            eyedropper = new Eyedropper();
            this.Click += EyedropperToolButton_Click;
            Window.Current.SizeChanged += Current_SizeChanged;
            eyedropper.PickEnded += Eyedropper_PickEnded;
        }
        private void OnIsEnabledChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (IsEnabled)
            {
                if (IsPressed)
                {
                    VisualStateManager.GoToState(this, EyedropperEnabled ? EyedropperEnabledPressedState : PressedState, true);
                }
                else if(IsPointerOver)
                {
                    VisualStateManager.GoToState(this, EyedropperEnabled ? EyedropperEnabledPointerOverState : PointerOverState, true);
                }
                else
                {
                    VisualStateManager.GoToState(this, EyedropperEnabled ? EyedropperEnabledState : NormalState, true);
                }
            }
            else
            {
                VisualStateManager.GoToState(this, EyedropperEnabled ? EyedropperEnabledDisabledState : DisabledState, true);
            }
        }

        private void EyedropperToolButton_Click(object sender, RoutedEventArgs e)
        {
            EyedropperEnabled = !EyedropperEnabled;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            UpadateEyedropperWorkArea();
        }
        
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
            EyedropperEnabled = false;
        }
    }
}
