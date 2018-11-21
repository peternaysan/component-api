using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.Acknowledgements;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class AesGetsResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public AckGetsReponse ackGetsReponse { get; set; }
    }
}
