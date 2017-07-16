using Dashboard.ViewModels;
using System.Windows.Controls;

namespace Dashboard.Views
{
    /// <summary>
    /// OverView.xaml 的交互逻辑
    /// </summary>
    public partial class OverView : UserControl
    {
        public OverView()
        {
            InitializeComponent();
            DataContext = new OverViewViewModel();
        }

        private void IoNavItem_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Page ioPage = new IOPage();
            MainFrame.Navigate(ioPage);
        }

        private void LatencyItem_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Page page = new LatencyPage();
            MainFrame.Navigate(page);
        }

        private void CapacityItem_Clicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Page page = new CapacityPage();
            MainFrame.Navigate(page);
        }
    }
}