using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using WWGachaExport.Models;
using WWGachaExport.Services;
using WWGachaExport.ViewModels.Dialogs;
using WWGachaExport.Views;
using WWGachaExport.Views.Dialogs;

namespace WWGachaExport.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        private readonly ConfigService _configService;
        private readonly GameUserService _gameUserService;
        private readonly IDialogService _dialogService;
        private GameUser _selectdUser;

        public ObservableCollection<GameUser> Users { get; set; } = new ObservableCollection<GameUser>();
        public ObservableCollection<GachaDataViewModel> GachaData { get; set; } = new ObservableCollection<GachaDataViewModel>();

        public GameUser SelectdUser
        {
            get => _selectdUser;
            set
            {
                SetProperty(ref _selectdUser, value);
                
                _configService.SelectedUID = _selectdUser == null ? 0 : _selectdUser.UID;
                _configService.SaveConfig();

                GachaData.Clear();
                if (_selectdUser != null)
                {
                    foreach (var poolData in _selectdUser.GachaPoolData)
                    {
                        if (_configService.HiddenNoobPool &&
                            _configService.GachaPools.FirstOrDefault(x => x.PoolType == poolData.PoolType).NoobPool)
                            continue;

                        GachaData.Add(new GachaDataViewModel()
                        {
                            PoolType = poolData.PoolType,
                            GachaData = new ObservableCollection<GachaData>(poolData.Data)
                        });
                    }
                }
            }
        }

        public RelayCommand ConfigCommand => new RelayCommand(() =>
        {
            var window = new ConfigWindow();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            if (_configService.NeedRefresh)
            {
                _configService.NeedRefresh = false;
                RefreshUsers();
            }
            _configService.SaveConfig();
        });

        public RelayCommand UpdateGachaData => new RelayCommand(() =>
        {
            /*
            var window = new UpdateGachaDataWindow();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
            RefreshUsers();
            */

            _dialogService.ShowDialog(
                this, 
                new UpdateGachaDataDialogViewModel(_configService, _gameUserService)
            );
            RefreshUsers();
        });

        public RelayCommand UpdateGachaDataFromUrl => new RelayCommand(() =>
        {
            var inputUrlDialogViewModel = new InputUrlDialogViewModel();
            bool? success = _dialogService.ShowDialog(this, inputUrlDialogViewModel);
            if (success == true)
            {
                _dialogService.ShowDialog(
                    this, 
                    new UpdateGachaDataDialogViewModel(_configService, _gameUserService, false, inputUrlDialogViewModel.Url)
                );
                RefreshUsers();
            }
        });

        public RelayCommand ExportGachaData => new RelayCommand(async () =>
        {
            if (SelectdUser == null)
                return;

            var dialog = new VistaSaveFileDialog();
            dialog.Filter = "excel (*.xlsx)|*.xlsx";
            dialog.DefaultExt = "xlsx";
            dialog.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if ((bool)dialog.ShowDialog())
            {
                try
                {
                    if (File.Exists(dialog.FileName))
                        File.Delete(dialog.FileName);

                    var excel = new ExcelPackage(new FileInfo(dialog.FileName));
                    foreach (var poolData in SelectdUser.GachaPoolData)
                    {
                        var workSheet = excel.Workbook.Worksheets.Add(_configService.GachaPools.FirstOrDefault(x => x.PoolType == poolData.PoolType).Name);
                        workSheet.Cells[1, 1, 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        workSheet.Cells[1, 1, 1, 6].Style.Font.Bold = true;
                        workSheet.Cells[1, 1, 1, 6].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        workSheet.Cells[1, 1, 1, 6].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0, 176, 240));
                        workSheet.Cells[1, 1, 1, 6].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        workSheet.Cells[1, 1, 1, 6].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        workSheet.Cells[1, 1, 1, 6].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        workSheet.Cells[1, 1, 1, 6].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        
                        workSheet.Column(1).Width = 20;
                        workSheet.Column(3).Width = 20;

                        workSheet.Cells[1, 1].Value = "时间";
                        workSheet.Cells[1, 2].Value = "类别";
                        workSheet.Cells[1, 3].Value = "名称";
                        workSheet.Cells[1, 4].Value = "品级";
                        workSheet.Cells[1, 5].Value = "次数";
                        workSheet.Cells[1, 6].Value = "保底";

                        int row = 2, count = 1, fiveCurrentCount = 1;
                        foreach (var gachaData in poolData.Data)
                        {
                            if (gachaData.QualityLevel == 5)
                            {
                                workSheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
                                workSheet.Cells[row, 1, row, 6].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(237, 125, 49));
                            }
                            else if (gachaData.QualityLevel == 4)
                            {
                                workSheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
                                workSheet.Cells[row, 1, row, 6].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(112, 48, 160));
                            }
                            else
                            {
                                workSheet.Cells[row, 1, row, 6].Style.Font.Color.SetColor(System.Drawing.Color.FromArgb(128, 128, 128));
                            }

                            workSheet.Cells[row, 1].Value = gachaData.Time.ToString("yyyy/MM/dd hh:mm:ss");
                            workSheet.Cells[row, 2].Value = gachaData.ResourceType;
                            workSheet.Cells[row, 3].Value = gachaData.Name;
                            workSheet.Cells[row, 4].Value = gachaData.QualityLevel;
                            workSheet.Cells[row, 5].Value = count;
                            workSheet.Cells[row, 6].Value = fiveCurrentCount;
                            row++;
                            count++;
                            fiveCurrentCount++;

                            if (gachaData.QualityLevel == 5)
                            {
                                fiveCurrentCount = 1;
                            }
                        }
                    }
                    excel.Save();
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "提示",
                        Content = "数据导出完成。",
                        CloseButtonText = "确定"
                    };
                    _ = await messageBox.ShowDialogAsync();
                }
                catch
                {
                    var messageBox = new Wpf.Ui.Controls.MessageBox
                    {
                        Title = "提示",
                        Content = "数据导出失败，请检查要保存的位置是否被占用。",
                        CloseButtonText = "确定"
                    };
                    _ = await messageBox.ShowDialogAsync();
                }
            }
        });
        public MainWindowViewModel(ConfigService configService, GameUserService gameUserService, IDialogService dialogService)
        {
            _configService = configService;
            _gameUserService = gameUserService;
            _dialogService = dialogService;
            _configService.LoadConfig();
            RefreshUsers();
        }

        private void RefreshUsers()
        {
            var selectedUID = _configService.SelectedUID;
            Users.Clear();
            foreach (var user in _gameUserService.GetUsers())
            {
                Users.Add(user);
            }
            // Users = new ObservableCollection<GameUser>(_gameUserService.GetUsers());
            SelectdUser = Users.FirstOrDefault(x => x.UID == selectedUID);
        }
    }
}
