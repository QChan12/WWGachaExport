using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWGachaExport.Models
{
    public class GachaData
    {
        public string CardPoolType { get; set; }
        public int ResourceId { get; set; }
        public int QualityLevel { get; set; } 
        public string ResourceType { get; set; }
        public string Name {  get; set; }
        public int Count { get; set; }
        public DateTime Time { get; set; }
    }
}
