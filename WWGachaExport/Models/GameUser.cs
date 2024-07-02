using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWGachaExport.Models
{
    public class GameUser
    {
        public long UID { get; set; }
        public string ServerID { get; set; }
        public string ServerArea { get; set; }
        public List<GachaPoolData> GachaPoolData { get; set; }
    }
}
