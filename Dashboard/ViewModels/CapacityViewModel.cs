using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using Prism.Mvvm;

namespace Dashboard.ViewModels
{
    public class CapacityViewModel : BindableBase
    {
        public CapacityViewModel()
        {
            CapacitySeriesCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Used",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(10) },
                    DataLabels = false,
                    Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#28A4FF"))
                },
                new PieSeries
                {
                    Title = "Unused",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(10) },
                    DataLabels = false,
                    Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#969696"))
                },
                new PieSeries
                {
                    Title = "UnAvaliable",
                    Values = new ChartValues<ObservableValue> { new ObservableValue(1) },
                    DataLabels = false,
                    Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("red"))
                }
            };
        }

        public SeriesCollection CapacitySeriesCollection { get; set; }
    }
}
