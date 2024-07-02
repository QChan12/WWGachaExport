using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WWGachaExport.Views;

namespace WWGachaExport.ViewModels
{
    public class ViewModelLocator
    {
        public MainWindowViewModel MainWindow => Ioc.Default.GetService<MainWindowViewModel>();
        public ConfigWindowViewModel ConfigWindow => Ioc.Default.GetService<ConfigWindowViewModel>();
        public UpdateGachaDataWindowViewModel UpdateGachaDataWindow => Ioc.Default.GetService<UpdateGachaDataWindowViewModel>();
    }
}
