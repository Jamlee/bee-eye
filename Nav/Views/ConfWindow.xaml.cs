using Nav.ViewModels;
using System.Windows;

namespace Nav.Views
{
    /// <summary>
    /// ConfWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ConfWindow : Window
    {
        public ConfWindow()
        {
            InitializeComponent();
            DataContext = new ConfWindowsViewModel();
        }
    }
}
