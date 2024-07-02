using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WWGachaExport.Models;

namespace WWGachaExport.Services
{
    public class GameUserService
    {
        private readonly string PathUserData = Path.Combine(Directory.GetCurrentDirectory(), "UserData");

        public GameUserService() 
        {
        }

        public List<GameUser> GetUsers()
        {
            var result = new List<GameUser>();
            if (Directory.Exists(PathUserData))
            {
                foreach (var filename in Directory.GetFiles(PathUserData, "*.json"))
                {
                    if (Path.GetFileName(filename).StartsWith("gacha-user-"))
                    {
                        var user = GetUser(filename);
                        if (user != null)
                        {
                            result.Add(user);
                        }
                    }
                }
            }
            return result;
        }

        public GameUser GetUser(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            GameUser user = null;
            try
            {
                user = JsonConvert.DeserializeObject<GameUser>(File.ReadAllText(fileName));
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            return user;
        }

        public GameUser GetUser(string serverArea, long uid)
        {
            return GetUser(Path.Combine(PathUserData, GetUserFileName(serverArea, uid)));
        }

        public bool SaveUser(GameUser user)
        {
            if (user == null)
                return false;

            try
            {
                if (!Directory.Exists(PathUserData))
                    Directory.CreateDirectory(PathUserData);

                File.WriteAllText(Path.Combine(PathUserData, GetUserFileName(user)), JsonConvert.SerializeObject(user));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
                return false;
            }
            return true;
        }

        public bool RemoveUser(GameUser user)
        {
            var filename = Path.Combine(PathUserData, GetUserFileName(user));
            if (user == null || !File.Exists(filename))
                return false;

            File.Delete(filename);
            return true;
        }

        public string GetUserFileName(string serverArea, long uid)
        {
            return $"gacha-user-{serverArea}-{uid}.json";
        }
        public string GetUserFileName(GameUser user)
        {
            return $"gacha-user-{user.ServerArea}-{user.UID}.json";
        }
    }
}
