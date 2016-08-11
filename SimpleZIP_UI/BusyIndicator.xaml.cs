using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SimpleZIP_UI
{
    public sealed partial class BusyIndicator : UserControl
    {
        private BusyIndicator(string title)
        {
            this.InitializeComponent();
            this.TitleTextBlock.Text = title;
        }

        /// <summary>
        /// Closes the BusyIndicator.
        /// </summary>
        public void Close()
        {
            ((Popup)Parent).IsOpen = false; // close the parent; closes the dialog too
        }

        /// <summary>
        /// Locks the screen and starts the BusyIndicator by creating a popup.
        /// </summary>
        /// <param name="title">The title to be displayed by the BusyIndicator</param>
        /// <returns>The BusyIndicator</returns>
        public static BusyIndicator Start(string title)
        {
            // create a popup with the size of the app's window
            var popup = new Popup()
            {
                Height = Window.Current.Bounds.Height,
                IsLightDismissEnabled = false,
                Width = Window.Current.Bounds.Width
            };

            // create the BusyIndicator as a child, having the same size as the app
            var busyIndicator = new BusyIndicator(title)
            {
                Height = popup.Height,
                Width = popup.Width
            };

            // set the child of the popup
            popup.Child = busyIndicator;

            // position the popup to the upper left corner
            popup.SetValue(Canvas.LeftProperty, 0);
            popup.SetValue(Canvas.TopProperty, 0);

            popup.IsOpen = true;

            return (busyIndicator);
        }
    }
}
