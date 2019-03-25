using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class ShipmentParty
    {
        [JsonProperty("partyId")]
        public string PartyId { get; set; }

        [JsonProperty("partyIdType")]
        public string PartyIdType { get; set; }

        [JsonProperty("partyType")]
        public string PartyType { get; set; }

        [JsonProperty("partyName")]
        public string PartyName { get; set; }

        [JsonProperty("contactFirstName")]
        public string ContactFirstName { get; set; }

        [JsonProperty("contactLastName")]
        public string ContactLastName { get; set; }

        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("contactPhoneNumber")]
        public string ContactPhoneNumber { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("stateCode")]
        public string StateCode { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("ultimateConsigneeType")]
        public string UltimateConsigneeType { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("usppiirsNumber")]
        public string UsppiirsNumber { get; set; }

        [JsonProperty("usppiirsidType")]
        public string UsppiirsidType { get; set; }

        [JsonProperty("checkForDeniedParty")]
        public string CheckForDeniedParty { get; set; }

        [JsonProperty("toBeSoldenRouteIndicator")]
        public string ToBeSoldenRouteIndicator { get; set; }

        [JsonProperty("consigneeFromGf")]
        public string consigneeFromGf { get; set; }
    }
}
