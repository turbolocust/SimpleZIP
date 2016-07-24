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
