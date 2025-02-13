using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WWGachaExport.Models;
using WWGachaExport.Services;

namespace WWGachaExport.ViewModels
{
    public class ConfigWindowViewModel : ObservableObject
    {
        private ConfigService _configService;
        private GameUserService _gameUserService;

        private string _pathGame;
        public string PathGame
        {
            get => _pathGame;
            set
            {
                SetProperty(ref _pathGame, value);
                _configService.PathGame = value;
            }
        }

        private bool _ignoreNoobPool;
        public bool IgnoreNoobPool
        {
            get => _ignoreNoobPool;
            set
            {
                SetProperty(ref _ignoreNoobPool, value);

                if (value != _configService.HiddenNoobPool)
                    _configService.NeedRefresh = true;

                _configService.HiddenNoobPool = value;
            }
        }


        public ObservableCollection<GameUser> Users { get; set; }

        private GameUser _selectdUser;
        public GameUser SelectdUser
        {
            get => _selectdUser;
            set => SetProperty(ref _selectdUser, value);
        }

        public RelayCommand SelectPathGameCommand => new RelayCommand(() =>
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "请选择游戏根目录";
            dialog.UseDescriptionForTitle = true;
            if (string.IsNullOrEmpty(PathGame) || !Directory.Exists(PathGame))
                dialog.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
            else
                dialog.SelectedPath = PathGame;
            if ((bool)dialog.ShowDialog())
            {
                PathGame = dialog.SelectedPath;
            }
        });

        public RelayCommand RemoveUserCommand => new RelayCommand(async () =>
        {
            if (SelectdUser == null)
                return;

            var messageBox = new Wpf.Ui.Controls.MessageBox
            {
                Title = "删除记录",
                Content = $"你确定要删除 uid：{SelectdUser.UID} 的记录吗？",
                PrimaryButtonText = "删除",
                CloseButtonText = "取消",
            };
            if (await messageBox.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                _gameUserService.RemoveUser(SelectdUser);
                Users.Remove(SelectdUser);
                SelectdUser = null;
                _configService.NeedRefresh = true;
            }
        });

        public RelayCommand FixEngineConfig => new RelayCommand(async () =>
        {
            if (string.IsNullOrEmpty(_configService.PathGame) || !Directory.Exists(_configService.PathGame))
            {
                await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "提示",
                    Content = "游戏路径未设置，请先设置游戏路径。",
                    CloseButtonText = "确定",
                    IsPrimaryButtonEnabled = false
                }.ShowDialogAsync();
                return;
            }
            var pathOfficial = Path.Combine(_configService.PathGame, @"Wuthering Waves Game\Client\Saved\Config\WindowsNoEditor\Engine.ini");
            var pathWeGame = Path.Combine(_configService.PathGame, @"Client\Saved\Config\WindowsNoEditor\Engine.ini");
            string pathConfig = null;
            if (Directory.Exists(Path.GetDirectoryName(pathOfficial)))
            {
                pathConfig = pathOfficial;
            }
            else if (Directory.Exists(Path.GetDirectoryName(pathWeGame)))
            {
                pathConfig = pathWeGame;
            }
            if (pathConfig == null || !File.Exists(pathConfig))
            {
                await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "提示",
                    Content = "未找到对应路径，请检查程序设置中的游戏路径。",
                    CloseButtonText = "确定",
                    IsPrimaryButtonEnabled = false
                }.ShowDialogAsync();
                return;
            }

            string[] configLines = null;
            try
            {
                configLines = File.ReadAllLines(pathConfig, Encoding.UTF8);
            }
            catch
            {
                await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "提示",
                    Content = "配置文件被占用，请关闭游戏后重试。",
                    CloseButtonText = "确定",
                    IsPrimaryButtonEnabled = false
                }.ShowDialogAsync();
                return;
            }

            StringBuilder outputConfig = new StringBuilder();
            string tag = null;
            foreach (var line in configLines)
            {
                if (line.StartsWith("["))
                {
                    tag = line;
                }
                if (tag != "[Core.Log]" || !line.ToLower().StartsWith("global=") && !line.ToLower().StartsWith("loginteractiveprocess="))
                {
                    outputConfig.AppendLine(line);
                }
            }

            try
            {
                File.WriteAllText(pathConfig, outputConfig.ToString());
            }
            catch
            {
                await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "提示",
                    Content = "修改配置文件失败，请关闭游戏后重试。",
                    CloseButtonText = "确定",
                    IsPrimaryButtonEnabled = false
                }.ShowDialogAsync();
            }
            finally
            {
                await new Wpf.Ui.Controls.MessageBox
                {
                    Title = "提示",
                    Content = "修改配置文件完成。",
                    CloseButtonText = "确定",
                    IsPrimaryButtonEnabled = false
                }.ShowDialogAsync();
            }
        });

        public ConfigWindowViewModel(ConfigService configService, GameUserService gameUserService) 
        {
            _configService = configService;
            _gameUserService = gameUserService;

            PathGame = configService.PathGame;
            IgnoreNoobPool = configService.HiddenNoobPool;
            Users = new ObservableCollection<GameUser>(gameUserService.GetUsers());
        }
    }
}
