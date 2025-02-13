using CommunityToolkit.Mvvm.ComponentModel;
using MvvmDialogs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WWGachaExport.Models;
using WWGachaExport.Services;

namespace WWGachaExport.ViewModels.Dialogs
{
    public class UpdateGachaDataDialogViewModel : ObservableObject, IModalDialogViewModel
    {
        private bool? _dialogResult;
        public bool? DialogResult
        {
            get => _dialogResult;
            private set => SetProperty(ref _dialogResult, value);
        }

        private string _logText;
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        private ConfigService _configService;
        private GameUserService _gameUserService;
        private bool _autoUrl;
        private string _url;

        public UpdateGachaDataDialogViewModel(ConfigService configService, GameUserService gameUserService, bool autoUrl = true, string url = null)
        {
            _configService = configService;
            _gameUserService = gameUserService;
            _autoUrl = autoUrl;
            _url = url;

            LogText = null;
            UpdateGachaData();
        }

        private async void UpdateGachaData()
        {
            string url = null;

            if (_autoUrl) 
            {
                AddLog("正在获取 URL ...");
                if (string.IsNullOrEmpty(_configService.PathGame) || !Directory.Exists(_configService.PathGame))
                {
                    AddLog("游戏路径设置有误，请检查程序设置中的游戏路径。");
                    return;
                }
                var pathLogOfficial = Path.Combine(_configService.PathGame, @"Wuthering Waves Game\Client\Saved\Logs\Client.log");
                var pathLogWeGame = Path.Combine(_configService.PathGame, @"Client\Saved\Logs\Client.log");
                string pathLog = null;
                if (Directory.Exists(Path.GetDirectoryName(pathLogOfficial)))
                {
                    pathLog = pathLogOfficial;
                }
                else if (Directory.Exists(Path.GetDirectoryName(pathLogWeGame)))
                {
                    pathLog = pathLogWeGame;
                }
                if (pathLog == null)
                {
                    AddLog("未找到对应路径，请检查程序设置中的游戏路径。");
                    return;
                }
                if (!File.Exists(pathLog))
                {
                    AddLog("找不到 Log 文件，请在游戏中打开抽卡历史记录后重试。");
                    return;
                }
                string[] logLines = null;
                try
                {
                    using (var fs = new FileStream(pathLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        logLines = sr.ReadToEnd().Split('\n');
                    }
                }
                catch (Exception e)
                {
                    AddLog("读取 Log 文件失败，请关闭游戏中的抽卡历史记录后重试。" + e.Message);
                }

                foreach (var line in logLines.Reverse())
                {
                    var match = Regex.Match(line, "(https?.*/aki/gacha/index\\.html#/record[\\?=&\\w\\-]+)");
                    if (match.Success)
                    {
                        url = match.Groups[1].Value;
                        break;
                    }
                }
                if (string.IsNullOrWhiteSpace(url))
                {
                    AddLog("无法从 Log 文件中获取 Url，请在游戏中打开抽卡历史记录后重试。");
                    return;
                }
                AddLog("获取成功，开始获取抽卡数据。");
            }
            else
            {
                AddLog("正在处理手动输入的 url ...");
                Uri uriResult;
                bool vaildUri = Uri.TryCreate(_url, UriKind.Absolute, out uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!vaildUri)
                {
                    AddLog("获取失败，输入的 url 地址有误。");
                    return;
                }
                url = _url;
            }


            string cardPoolId = null, languageCode = null, recordId = null, serverId = null, serverArea = null;
            long playerId = 0;
            foreach (var param in url.Substring(url.IndexOf("?") + 1).Split('&'))
            {
                // resources_id = cardPoolId
                // gacha_type = cardPoolType
                // lang = languageCode
                // player_id = playerId
                // record_id = recordId
                // svr_id = serverId
                if (param.StartsWith("resources_id"))
                {
                    cardPoolId = param.Split('=')[1];
                }
                else if (param.StartsWith("lang"))
                {
                    languageCode = param.Split('=')[1];
                }
                else if (param.StartsWith("player_id"))
                {
                    playerId = long.Parse(param.Split('=')[1]);
                }
                else if (param.StartsWith("record_id"))
                {
                    recordId = param.Split('=')[1];
                }
                else if (param.StartsWith("svr_id"))
                {
                    serverId = param.Split('=')[1];
                }
                else if (param.StartsWith("svr_area"))
                {
                    serverArea = param.Split('=')[1];
                }
            }
            bool serverCN = serverArea != "global";
            _configService.SelectedUID = playerId;
            _configService.SaveConfig();

            var user = _gameUserService.GetUser(serverArea, playerId);
            if (user == null)
            {
                user = new GameUser()
                {
                    UID = playerId,
                    ServerID = serverId,
                    ServerArea = serverArea,
                };
                user.GachaPoolData = new List<GachaPoolData>();
                foreach (var gachaPool in _configService.GachaPools)
                {
                    user.GachaPoolData.Add(new GachaPoolData()
                    {
                        PoolType = gachaPool.PoolType,
                        Data = new List<GachaData>()
                    });
                }
            }

            if (_configService.HiddenNoobPool)
            {
                AddLog($"由于设置原因，本次获取跳过新手池");
            }


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");
                foreach (var gachaPool in _configService.GachaPools)
                {
                    if (gachaPool.NoobPool && _configService.HiddenNoobPool)
                        continue;

                    AddLog($"获取池子：{gachaPool.Name}");

                    var response = await client.PostAsync(
                        serverCN ? 
                        "https://gmserver-api.aki-game2.com/gacha/record/query" :
                        "https://gmserver-api.aki-game2.net/gacha/record/query",
                        new StringContent(
                            JsonConvert.SerializeObject(new
                            {
                                cardPoolId,
                                cardPoolType = gachaPool.PoolType,
                                languageCode,
                                playerId,
                                recordId,
                                serverId
                            }),
                            Encoding.UTF8,
                            "application/json"
                        )
                    );

                    var getSuccess = false;
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        try
                        {
                            var apiData = JsonConvert.DeserializeObject<GachaAPI>(
                                responseJson,
                                new JsonSerializerSettings()
                                {
                                    DateFormatString = "yyyy/MM/dd hh:mm:ss"
                                }
                            );
                            if (apiData.Code == 0 && apiData.Message == "success")
                            {
                                getSuccess = true;
                                apiData.Data.Reverse();
                                DateTime lastTime = DateTime.MinValue;

                                var gachaPoolData = user.GachaPoolData.FirstOrDefault(x => x.PoolType == gachaPool.PoolType);
                                if (gachaPoolData.Data.Count > 0)
                                {
                                    lastTime = gachaPoolData.Data.Last().Time;
                                }
                                foreach (var item in apiData.Data)
                                {
                                    if (lastTime >= item.Time)
                                    {
                                        //AddLog($"跳过: {item.QualityLevel} | {item.Name} | {item.Time.ToString("yyyy/MM/dd hh::mm:ss")}");
                                        continue;
                                    }
                                    gachaPoolData.Data.Add(new GachaData
                                    {
                                        CardPoolType = item.CardPoolType,
                                        ResourceId = item.ResourceId,
                                        QualityLevel = item.QualityLevel,
                                        ResourceType = item.ResourceType,
                                        Name = item.Name,
                                        Count = item.Count,
                                        Time = item.Time
                                    });
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            AddLog(e.Message);
                        }
                    }
                    if (!getSuccess)
                    {
                        AddLog($"获取失败：{gachaPool.Name}");
                    }
                }
            }
            AddLog($"获取完成, 正在保存记录...");
            if (_gameUserService.SaveUser(user))
            {
                AddLog($"更新完成");
                DialogResult = true;
            }
            else
            {
                AddLog($"记录保存失败");
            }
        }

        private void AddLog(string text)
        {
            LogText += text + "\n";
        }
    }
}