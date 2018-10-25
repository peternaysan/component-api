using System.Collections.Generic;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class Country
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("states")]
        public List<State> States { get; set; }
    }
}
