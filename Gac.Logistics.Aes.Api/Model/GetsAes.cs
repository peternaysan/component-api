using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class GetsAes
    {
        public GetsAes()
        {
        }

        [JsonProperty("header")]
        public AesHeader Header { get; set; }

        [JsonProperty("shipmentHeader")]
        public ShipmentHeader ShipmentHeader { get; set; }

        [JsonProperty("shipmentParty")]
        public List<ShipmentParty> ShipmentParty { get; set; }

        [JsonProperty("transportation")]
        public Transportation Transportation { get; set; }

        [JsonProperty("commodityDetails")]
        public List<CommodityDetails> CommodityDetails { get; set; }       

        [JsonProperty("submissionResponse")]
        public SubmissionStatus SubmissionResponse { get; set; }

        [JsonProperty("submissionStatus")]
        public string SubmissionStatus { get; set; }

        [JsonProperty("submissionStatusDescription")]
        public string SubmissionStatusDescription { get; set; }
        [JsonProperty("statusNotification")]
        public List<StatusNotification> StatusNotification { get; set; }

    }
}
