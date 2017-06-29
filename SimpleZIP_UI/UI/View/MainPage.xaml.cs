using System;
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
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private void OpenArchiveButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opens the project's homepage in the web browser. 
        /// Brings up a confirmation dialog first to avoid accidential redirection. 
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private async void GetSourceButton_Tap(object sender, TappedRoutedEventArgs args)
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
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that may have been passed.</param>
        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await new AboutDialog().ShowAsync();
        }

        /// <summary>
        /// Invoked after navigating to this page.
        /// </summary>
        /// <param name="args">Arguments that may have been passed.</param>
        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            var frame = Window.Current.Content as Frame;
            frame?.BackStack.Clear(); // going back is prohibited after aborting operation
        }
    }
}
