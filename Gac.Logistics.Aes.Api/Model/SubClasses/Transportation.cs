using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class Transportation
    {
        [JsonProperty("modeofTransport")]
        public string ModeofTransport { get; set; }

        [JsonProperty("carrierCode")]
        public string CarrierCode { get; set; }

        [JsonProperty("vesselName")]
        public string VesselName { get; set; }

        [JsonProperty("transportationDetails", NullValueHandling = NullValueHandling.Ignore)]
        public List<TransportationDetails> TransportationDetails { get; set; }
    }



}
