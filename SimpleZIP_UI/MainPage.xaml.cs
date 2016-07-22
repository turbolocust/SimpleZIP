using System;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleZIP_UI
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

        private void ShowFileExplorer(object sender, RoutedEventArgs e)
        {
            //FontFamily="Segoe MDL2 Assets"
        }

        private void HamburgerButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void CompressButton_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void ExtractButton_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void HomeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void SettingsButton_Tap(object sender, TappedRoutedEventArgs e)
        {

        }

        private void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
