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
    public partial class EyedropperToolButton: ButtonBase
    {
        private const string NormalState = "Normal";
        private const string PointerOverState = "PointerOver";
        private const string PressedState = "Pressed";
        private const string DisabledState = "Disabled";
        private const string EyedropperEnabledState = "EyedropperEnabled";
        private const string EyedropperEnabledPointerOverState = "EyedropperEnabledPointerOver";
        private const string EyedropperEnabledPressedState = "EyedropperEnabledPressed";
        private const string EyedropperEnabledDisabledState = "EyedropperEnabledDisabled";

        public event TypedEventHandler<EyedropperToolButton, ColorChangedEventArgs> ColorChanged;
        public event TypedEventHandler<EyedropperToolButton, EventArgs> PickEnded;
        public event TypedEventHandler<EyedropperToolButton, EventArgs> PickStarted;

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
            eyedropper.ColorChanged += Eyedropper_ColorChanged;
            eyedropper.PickStarted += Eyedropper_PickStarted;
            eyedropper.PickEnded += Eyedropper_PickEnded;
            this.Click += EyedropperToolButton_Click;
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void Eyedropper_PickStarted(Eyedropper sender, EventArgs args)
        {
            this.PickStarted?.Invoke(this,args);
        }

        private void Eyedropper_ColorChanged(Eyedropper sender, ColorChangedEventArgs args)
        {
            Color = args.NewColor;
            this.ColorChanged?.Invoke(this,args);
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

        private void UpadateEyedropperWorkArea()
        {
            if (Target != null)
            {
                var transform = Target.TransformToVisual(Window.Current.Content);
                var position = transform.TransformPoint(new Point());
                eyedropper.WorkArea = new Rect(position, new Size(Target.ActualWidth, Target.ActualHeight));
            }
        }

        private void Eyedropper_PickEnded(Eyedropper sender, EventArgs args)
        {
            EyedropperEnabled = false;
            this.PickEnded?.Invoke(this, args);
        }
    }
}
