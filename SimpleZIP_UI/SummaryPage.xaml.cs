using Windows.UI.Xaml.Controls;
using SimpleZIP_UI.Control;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace SimpleZIP_UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SummaryPage : Page
    {
        private readonly SummaryPageControl _control;

        public SummaryPage()
        {
            this.InitializeComponent();
            _control = new SummaryPageControl();
        }
    }
}
