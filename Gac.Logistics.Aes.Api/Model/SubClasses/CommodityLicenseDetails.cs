using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class CommodityLicenseDetails
    {

        [JsonProperty("licenseExemptionCode")]
        public string LicenseExemptionCode { get; set; }

        [JsonProperty("exportLicenseNumber")]
        public string ExportLicenseNumber { get; set; }
    }
}
