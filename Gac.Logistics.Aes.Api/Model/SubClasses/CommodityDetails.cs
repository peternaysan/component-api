using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.SubClasses;

namespace Gac.Logistics.Aes.Api.Model
{
    public class CommodityDetails
    {
        public CommodityLineDetails[] CommodityLineDetails { get; set; }

        public CommodityLicenseDetails[] LicenseDetails { get; set; }
    }
  
    
}
