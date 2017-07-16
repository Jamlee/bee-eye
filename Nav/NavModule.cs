using Nav.Views;
using Prism.Modularity;
using Prism.Regions;
using System;

namespace Nav
{
    public class NavModule : IModule
    {
        IRegionManager _regionManager;

        public NavModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void Initialize()
        {
            _regionManager.RegisterViewWithRegion("NavRegion", typeof(TopMenu));
        }
    }
}