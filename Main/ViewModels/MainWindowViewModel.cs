using Prism.Mvvm;

namespace Bee.Eye.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Linux 监控程序 | Ubuntu/Centos7";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
