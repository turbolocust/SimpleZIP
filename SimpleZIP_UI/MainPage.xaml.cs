using System;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SimpleZIP_UI.Control;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SimpleZIP_UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainPageControl _control;

        public MainPage()
        {
            this.InitializeComponent();
            _control = new MainPageControl();
        }

        private void ShowFileExplorer(object sender, RoutedEventArgs e)
        {
            //FontFamily="Segoe MDL2 Assets"
        }

        private void HamburgerButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            this.MenuSplitView.IsPaneOpen = !this.MenuSplitView.IsPaneOpen;
        }

        private void CompressButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.CompressButtonAction();
        }

        private void ExtractButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.DecompressButtonAction();
        }

        /// <summary>
        /// Opens the project webpage in the default browser. 
        /// Brings up a confirmation dialog to avoid accidential termination of application first. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetSourceButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            var dialog = new MessageDialog("This will redirect you to the Web-Browser.\n\nAre you sure?");
            dialog.Commands.Add(new UICommand("Yes") { Id = 0 });
            dialog.Commands.Add(new UICommand("No") { Id = 1 });
            dialog.DefaultCommandIndex = 0;
            dialog.CancelCommandIndex = 1;

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // launch browser
            {
                await Launcher.LaunchUriAsync(new Uri("https://github.com/turbolocust/SimpleZIP"));
            }

        }

        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            await new AboutDialog().ShowAsync();
        }
    }
}
