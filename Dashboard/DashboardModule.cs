using Dashboard.Views;
using Prism.Events;
using Prism.Regions;
using Microsoft.Practices.ServiceLocation;

namespace Dashboard
{
    public class DashboardModule : Prism.Modularity.IModule
    {
        IRegionManager _regionManager;
        IServiceLocator _sl;

        public DashboardModule(IRegionManager regionManager, IEventAggregator eventAggregator,  IServiceLocator sl)
        {
            _regionManager = regionManager;
            _sl = sl;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("MainRegion", typeof(OverView));
        }
    }
}