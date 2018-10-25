using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class Aes
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("bookingId")]
        public long BookingId { get; set; }

        [JsonProperty("instanceCode")]
        public string InstanceCode { get; set; }

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

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("submittedOn")]
        public DateTime? SubmittedOn { get; set; }

        public SubmissionStatus SubmissionStatus { get; set; }
    }
}
