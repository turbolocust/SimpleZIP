using System;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Exceptions;
using SimpleZIP_UI.UI.Factory;

namespace SimpleZIP_UI.UI.View
{
    public sealed partial class MainPage
    {
        private readonly MainPageControl _control;

        public MainPage()
        {
            InitializeComponent();
            _control = new MainPageControl(this);
        }

        private void HamburgerButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        private void CompressButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            _control.CompressButtonAction();
        }

        private async void ExtractButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                _control.DecompressButtonAction();
            }
            catch (InvalidArchiveTypeException ex)
            {
                var dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sender">The sender that invoked this event.</param>
        /// <param name="e"></param>
        private void OpenArchiveButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens the project webpage in the default browser. 
        /// Brings up a confirmation dialog to avoid accidential termination of application first. 
        /// </summary>
        /// <param name="sender">The sender that invoked this event.</param>
        /// <param name="e"></param>
        private async void GetSourceButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            var dialog = DialogFactory.CreateConfirmationDialog("",
                "This will redirect you to the web browser.\n\nDo you want to proceed?");

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // launch browser
            {
                await Launcher.LaunchUriAsync(new Uri("https://github.com/turbolocust/SimpleZIP"));
            }
        }

        /// <summary>
        /// Shows a dialog with information about the application.
        /// </summary>
        /// <param name="sender">The sender that invoked this event.</param>
        /// <param name="e"></param>
        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            await new AboutDialog().ShowAsync();
        }

        /// <summary>
        /// Invoked after navigating to this page.
        /// </summary>
        /// <param name="args">The arguments of the navigation event.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs args)
        {
            var frame = Window.Current.Content as Frame;
            frame?.BackStack.Clear(); // going back is prohibited after aborting operation
            await Task.Delay(200); // avoid triggering of another tap event
        }
    }
}
