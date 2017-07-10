using System;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleZIP_UI.Presentation.Factory;
using Windows.UI.ViewManagement;
using Windows.Foundation;
using Windows.Storage;
using SimpleZIP_UI.Application.Compression.Model;
using SimpleZIP_UI.Application.Util;
using SimpleZIP_UI.Presentation.Control;

namespace SimpleZIP_UI.Presentation.View
{
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Constant which defines the preferred width of the view.
        /// </summary>
        private const double PreferredLaunchSizeWidth = 400d;

        /// <summary>
        /// Constant which defines the preferred height of the view.
        /// </summary>
        private const double PreferredLaunchSizeHeight = 600d;

        /// <summary>
        /// The aggregated control instance.
        /// </summary>
        private readonly MainPageControl _control;

        public MainPage()
        {
            InitializeComponent();
            _control = new MainPageControl(this);
            // set default launch size (will have no effect on phones)
            ApplicationView.PreferredLaunchViewSize = new Size(PreferredLaunchSizeWidth, PreferredLaunchSizeHeight);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
        }

        private void HamburgerButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = !MenuSplitView.IsPaneOpen;
        }

        private async void CompressButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            await _control.CompressButtonAction();
        }

        private async void ExtractButton_Tap(object sender, TappedRoutedEventArgs e)
        {
            await _control.DecompressButtonAction();
        }

        /// <summary>
        /// Opens an archive for exploring.
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        private async void OpenArchiveButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await _control.OpenArchiveButtonAction();
        }

        /// <summary>
        /// Opens the project's homepage using the <see cref="Launcher"/>.
        /// Brings up a confirmation dialog first to avoid accidental redirection. 
        /// </summary>
        /// <param name="sender">The sender of this event.</param>
        /// <param name="args">Arguments that have been passed.</param>
        private async void GetSourceButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            var dialog = DialogFactory.CreateConfirmationDialog(
                "This will open the web browser.", "\nProceed?");

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
        /// <param name="args">Arguments that have been passed.</param>
        private async void AboutMenuButton_Tap(object sender, TappedRoutedEventArgs args)
        {
            await new AboutDialog().ShowAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs args)
        {
            if (Window.Current.Content is Frame frame)
            {
                var eventArgs = args.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
                if (eventArgs?.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    var fileArgs = eventArgs as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                    var files = fileArgs?.Files;
                    if (!files.IsNullOrEmpty())
                    {
                        var list = new List<ExtractableItem>(files.Count);
                        list.AddRange(files.Select(file => new ExtractableItem(file.Name, file as StorageFile)));
                        frame.Navigate(typeof(DecompressionSummaryPage), list);
                    }
                }
                else
                {
                    frame.BackStack.Clear(); // going back is prohibited after aborting operation
                }
            }
        }
    }
}
