using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui;
using WWGachaExport.Services;
using WWGachaExport.ViewModels;
using WWGachaExport.Views;

namespace WWGachaExport
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<ConfigService>()
                .AddSingleton<GameUserService>()
                .AddSingleton<IContentDialogService, ContentDialogService>()
                .AddTransient<MainWindowViewModel>()
                .AddTransient<ConfigWindowViewModel>()
                .BuildServiceProvider());

            this.InitializeComponent();
        }
    }
}
