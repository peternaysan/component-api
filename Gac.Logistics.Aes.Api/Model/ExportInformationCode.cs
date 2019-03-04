using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class ExportInformationCode
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
