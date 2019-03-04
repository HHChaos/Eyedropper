using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;

namespace Eyedropper.UWP
{
    public sealed class ColorChangedEventArgs
    {
        public Color NewColor { internal set; get; }
        public Color OldColor { internal set; get; }
    }
}
