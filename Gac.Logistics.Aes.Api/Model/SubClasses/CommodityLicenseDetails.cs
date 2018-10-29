using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class CommodityLicenseDetails
    {

        [JsonProperty("licenseExemptionCode")]
        public string LicenseExemptionCode { get; set; }

        [JsonProperty("exportLicenseNumber")]
        public string ExportLicenseNumber { get; set; }

        [JsonProperty("pgaLicenseRequiredIndicator")]
        public string PGALicenseRequiredIndicator { get; set; }

        [JsonProperty("licenseValue")]
        public string LicenseValue { get; set; }

        [JsonProperty("eccn")]
        public string Eccn { get; set; }
    }
}
