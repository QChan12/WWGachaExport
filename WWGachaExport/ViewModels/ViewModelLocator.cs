using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WWGachaExport.ViewModels.Dialogs;
using WWGachaExport.Views;

namespace WWGachaExport.ViewModels
{
    public class ViewModelLocator
    {
        public MainWindowViewModel MainWindow => Ioc.Default.GetService<MainWindowViewModel>();
        public ConfigWindowViewModel ConfigWindow => Ioc.Default.GetService<ConfigWindowViewModel>();
        public InputUrlDialogViewModel InputUrlDialog => Ioc.Default.GetService<InputUrlDialogViewModel>();
        public UpdateGachaDataDialogViewModel UpdateGachaDataDialog => Ioc.Default.GetService<UpdateGachaDataDialogViewModel>();
    }
}
