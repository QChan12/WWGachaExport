using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WWGachaExport.Models
{
    public class GachaAPI
    {
        [JsonProperty("code")]
        public int Code;

        [JsonProperty("message")]
        public string Message;

        [JsonProperty("data")]
        public List<KRAPIItem> Data;
    }

    public class KRAPIItem
    {
        [JsonProperty("cardPoolType")]
        public string CardPoolType;

        [JsonProperty("resourceId")]
        public int ResourceId;

        [JsonProperty("qualityLevel")]
        public int QualityLevel;

        [JsonProperty("resourceType")]
        public string ResourceType;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("count")]
        public int Count;

        [JsonProperty("time")]
        public DateTime Time;
    }
}
