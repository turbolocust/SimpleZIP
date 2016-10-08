using System;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
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
            catch (InvalidFileTypeException ex)
            {
                var dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// 
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
                "This will redirect you to the Browser.\n\nAre you sure?");

            var result = await dialog.ShowAsync();
            if (result.Id.Equals(0)) // launch browser
            {
                await Launcher.LaunchUriAsync(new Uri("https://github.com/turbolocust/SimpleZIP"));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender">The sender that invoked this event.</param>
        /// <param name="e"></param>
        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            await new AboutDialog().ShowAsync();
        }
    }
}
