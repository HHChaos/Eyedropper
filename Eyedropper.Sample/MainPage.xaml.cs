﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Eyedropper.Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void GlobleBtn_Click(object sender, RoutedEventArgs e)
        {
            var eyedropper = new Eyedropper.UWP.Eyedropper();
            var color = await eyedropper.Open();
            Tip.Text=$"You get {color}.";
        }

        private void EyedropperToolButton_PickEnded(UWP.EyedropperToolButton sender, EventArgs args)
        {
            Tip.Text = $"You get {sender.Color}.";
        }
    }
}
