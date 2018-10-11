using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class AesExternal
    {
        [JsonProperty("AesExternal")]
        public Aes Aes { get; set; }
    }
}
