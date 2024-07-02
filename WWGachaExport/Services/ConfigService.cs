using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WWGachaExport.Models;

namespace WWGachaExport.Services
{
    public class ConfigService
    {
        private readonly string PathUserData = Path.Combine(Directory.GetCurrentDirectory(), "UserData");

        public string PathGame { get; set; }
        public bool HiddenNoobPool { get; set; }
        public long SelectedUID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        [JsonIgnore]
        public bool NeedRefresh { get; set; }

        [JsonIgnore]
        public List<GachaPoolInfo> GachaPools = new List<GachaPoolInfo>()
        {
            new GachaPoolInfo(1, "角色活动唤取", false, 80, 10),
            new GachaPoolInfo(2, "武器活动唤取", false, 80, 10),
            new GachaPoolInfo(3, "角色常驻唤取", false, 80, 10),
            new GachaPoolInfo(4, "武器常驻唤取", false, 80, 10),
            new GachaPoolInfo(5, "新手唤取", true, 50, 10),
            new GachaPoolInfo(6, "新手自选唤取", true, 80, 10),
            new GachaPoolInfo(7, "新手自选唤取（感恩定向唤取）", true, 1, 10),
        };
        
        public ConfigService() 
        {
            //LoadConfig();
        }

        public void LoadConfig()
        {
            try
            {
                if (!Directory.Exists(PathUserData))
                    Directory.CreateDirectory(PathUserData);

                var config = JsonConvert.DeserializeObject<ConfigService>(File.ReadAllText(Path.Combine(PathUserData, "Config.json")));
                this.PathGame = config.PathGame;
                this.HiddenNoobPool = config.HiddenNoobPool;
                this.SelectedUID = config.SelectedUID;
                this.Width = config.Width;
                this.Height = config.Height;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }

        public bool SaveConfig()
        {
            try
            {
                if (!Directory.Exists(PathUserData))
                    Directory.CreateDirectory(PathUserData);

                File.WriteAllText(Path.Combine(PathUserData, "Config.json"), JsonConvert.SerializeObject(this));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            return true;
        }
    }
}
