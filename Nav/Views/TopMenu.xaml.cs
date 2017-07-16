using System.Windows;
using System.Windows.Controls;

namespace Nav.Views
{
    /// <summary>
    /// TopMenu.xaml 的交互逻辑
    /// </summary>
    public partial class TopMenu : UserControl
    {
        public TopMenu()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewConnection_Click(object sender, RoutedEventArgs e)
        {
            ConfWindow subWindow = new ConfWindow();
            Window mainWindow = Application.Current.MainWindow;
            subWindow.Left = mainWindow.Left + (mainWindow.ActualWidth - subWindow.Width) / 2;
            subWindow.Top = mainWindow.Top + (mainWindow.ActualHeight - subWindow.Height) / 2;
            subWindow.ResizeMode = ResizeMode.NoResize;
            subWindow.Show();
        }
    }
}
