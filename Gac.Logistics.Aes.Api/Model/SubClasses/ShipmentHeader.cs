using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class ShipmentHeader
    {
        [JsonProperty("relatedParties")]
        public string RelatedParties { get; set; }

        [JsonProperty("filingTypeIndicator")]
        public string FilingTypeIndicator { get; set; }

        [JsonProperty("shipmentAction")]
        public string ShipmentAction { get; set; }

        [JsonProperty("shipmentReferenceNumber")]
        public string ShipmentReferenceNumber { get; set; }

        [JsonProperty("estimatedExportDate")]
        public string EstimatedExportDate { get; set; }

        [JsonProperty("portofExportation")]
        public string PortofExportation { get; set; }

        [JsonProperty("portofUnlading")]
        public string PortofUnlading { get; set; }

        [JsonProperty("inbondCode")]
        public string InbondCode { get; set; }

        [JsonProperty("entryNumber")]
        public string EntryNumber { get; set; }

        [JsonProperty("foreignTradeZone")]
        public string ForeignTradeZone { get; set; }

        [JsonProperty("originState")]
        public string OriginState { get; set; }

        [JsonProperty("ultimateDestinationCountry")]
        public string UltimateDestinationCountry { get; set; }

        [JsonProperty("fppiRoutedTransport")]
        public string FppiRoutedTransport { get; set; }

        [JsonProperty("hazmatIndicator")]
        public string HazmatIndicator { get; set; }
    }
}
