using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class HtsCode
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("uom1")]
        public string Uom1 { get; set; }
        [JsonProperty("uom2")]
        public string Uom2 { get; set; }

    }
}
