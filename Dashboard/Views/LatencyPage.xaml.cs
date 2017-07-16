using Dashboard.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dashboard.Views
{
    /// <summary>
    /// LatencyPage.xaml 的交互逻辑
    /// </summary>
    public partial class LatencyPage : Page
    {
        public LatencyPage()
        {
            InitializeComponent();
            DataContext = new LatencyViewModel();
            Unloaded += OnUnload;
        }

        public void OnUnload(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("LatencyPage unload");
            ((LatencyViewModel)DataContext).Cts.Cancel();
        }
    }
}
