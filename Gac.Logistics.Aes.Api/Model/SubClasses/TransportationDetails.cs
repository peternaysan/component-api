using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class TransportationDetails
    {
        [JsonProperty("equipmentNumber")]

        public string EquipmentNumber { get; set; }

        [JsonProperty("transportationReferenceNumber")]

        public string TransportationReferenceNumber { get; set; }
    }
}
