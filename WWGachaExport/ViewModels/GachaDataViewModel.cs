using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WWGachaExport.Models;
using WWGachaExport.Services;

namespace WWGachaExport.ViewModels
{
    public class GachaDataViewModel : ObservableObject
    {
        private ConfigService _configService;

        private int _PoolType;
        public int PoolType
        {
            get => _PoolType;
            set => SetProperty(ref _PoolType, value);
        }

        public string PoolName
        {
            get
            {
                return _configService.GachaPools.FirstOrDefault(x => x.PoolType == this.PoolType).Name;
            }
        }

        public ObservableCollection<GachaData> GachaData { get; set; }

        public string UpdateTime
        {
            get
            {
                if (GachaData == null)
                {
                    return "未更新";
                }
                return $"{GachaData.First().Time.ToString("yyyy/MM/dd")} - {GachaData.Last().Time.ToString("yyyy/MM/dd")}";
            }
        }

        public int TotalCount => this.GachaData == null ? 0 : GachaData.Count;
        public int LevelFiveCurrentCount
        {
            get
            {
                if (this.GachaData == null || this.GachaData.Count == 0)
                    return 0;

                var count = 0;
                foreach (var data in this.GachaData.Reverse())
                {
                    if (data.QualityLevel == 5)
                        break;

                    count++;
                }
                return count;
            }
        }
        public int LevelFourCurrentCount
        {
            get
            {
                if (this.GachaData == null || this.GachaData.Count == 0)
                    return 0;

                var count = 0;
                foreach (var data in this.GachaData.Reverse())
                {
                    if (data.QualityLevel == 4 || data.QualityLevel == 5)
                        break;

                    count++;
                }
                return count;
            }
        }
        public int LevelFiveCurrentMaxCount
        {
            get
            {
                return _configService.GachaPools.FirstOrDefault(x => x.PoolType == this.PoolType).LevelFiveMaxDraw;
            }
        }
        public int LevelFourCurrentMaxCount
        {
            get
            {
                return _configService.GachaPools.FirstOrDefault(x => x.PoolType == this.PoolType).LevelFourMaxDraw;
            }
        }
        public string LevelFiveCurrentText
        {
            get
            {
                return $"{LevelFiveCurrentCount}/{LevelFiveCurrentMaxCount}";
            }
        }
        public string LevelFourCurrentText
        {
            get
            {
                return $"{LevelFourCurrentCount}/{LevelFourCurrentMaxCount}";
            }
        }
        public double LevelFiveAverageCount
        {
            get
            {
                if (this.GachaData == null || this.GachaData.Count == 0)
                    return 0;

                var fiveLog = this.LevelFiveLog;
                if (fiveLog.Count == 0)
                    return 0;

                var total = 0;
                foreach (var log in fiveLog)
                {
                    total += log.Count;
                }
                return Math.Round((float)total / fiveLog.Count, 2);
            }
        }
        public int LevelFiveCount
        {
            get
            {
                if (this.GachaData == null)
                    return 0;

                return this.GachaData.Where(x => x.QualityLevel == 5).Count();
            }
        }
        public int LevelFourCount
        {
            get
            {
                if (this.GachaData == null)
                    return 0;

                return this.GachaData.Where(x => x.QualityLevel == 4).Count();
            }
        }
        public int LevelThreeCount
        {
            get
            {
                if (this.GachaData == null)
                    return 0;

                return this.GachaData.Where(x => x.QualityLevel == 3).Count();
            }
        }

        public string LevelFivePercentage
        {
            get
            {
                if (this.TotalCount == 0)
                    return "[0.00%]";
                return $"[{Math.Round((float)this.LevelFiveCount / this.TotalCount * 100f, 2)}%]";
            }
        }

        public string LevelFourPercentage
        {
            get
            {
                if (this.TotalCount == 0)
                    return "[0.00%]";
                return $"[{Math.Round((float)this.LevelFourCount / this.TotalCount * 100f, 2)}%]";
            }
        }

        public string LevelThreePercentage
        {
            get
            {
                if (this.TotalCount == 0)
                    return "[0.00%]";
                return $"[{Math.Round((float)this.LevelThreeCount / this.TotalCount * 100f, 2)}%]";
            }
        }

        public ObservableCollection<GachaFiveLogViewModel> LevelFiveLog
        {
            get
            {
                var result = new ObservableCollection<GachaFiveLogViewModel>();
                if (this.GachaData != null)
                {
                    var count = 0;
                    foreach (var gachaData in this.GachaData)
                    {
                        count++;
                        if (gachaData.QualityLevel == 5)
                        {
                            result.Add(new GachaFiveLogViewModel
                            {
                                Name = gachaData.Name,
                                Count = count,
                                Time = gachaData.Time.ToString("yyyy/MM/dd hh:mm:ss")
                            });
                            count = 0;
                        }
                    }
                }
                return result;
            }
        }

        public GachaDataViewModel()
        {
            _configService = Ioc.Default.GetService<ConfigService>();
        }
    }

    public class GachaFiveLogViewModel : ObservableObject
    {
        private string _name;
        private int _count;
        private string _time;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public int Count
        {
            get => _count;
            set => SetProperty(ref _count, value);
        }
        public string Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }
        public string Text
        {
            get
            {
                return $"{Name}[{Count}] ";
            }
        }
        public SolidColorBrush Color
        {
            get
            {
                if (Count <= 25)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ED7D31"));
                }
                else if (Count <= 50)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7030A0"));
                }
                else if (Count <= 73)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#a26b6e"));
                }
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#755f3b"));
            }
        }
    }
}
