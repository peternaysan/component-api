using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class AesTransaction
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("aesDetailEntity")]
        public Aes AesDetailEntity { get; set; }
        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }
      
    }
}
