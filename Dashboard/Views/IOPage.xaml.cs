using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Windows.Controls;
using Dashboard.ViewModels;
using System.Windows;

namespace Dashboard.Views
{
    /// <summary>
    /// IOPage.xaml 的交互逻辑
    /// </summary>
    public partial class IOPage : Page
    {
        public IOPage()
        {
            InitializeComponent();
            DataContext = new IOViewModel();
            Unloaded += OnUnload;
        }

        public void OnUnload(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("LatencyPage unload");
            ((IOViewModel)DataContext).Cts.Cancel();
        }
    }
}
