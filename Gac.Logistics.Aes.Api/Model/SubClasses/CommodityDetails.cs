using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Newtonsoft.Json;


namespace Gac.Logistics.Aes.Api.Model
{
    public class CommodityDetails
    {
        [JsonProperty("commodityLineDetails")]
        public CommodityLineDetails CommodityLineDetails { get; set; }

        [JsonProperty("licenseDetails")]
        public CommodityLicenseDetails LicenseDetails { get; set; }
    }
  
    
}
