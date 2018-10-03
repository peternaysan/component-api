using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.SubClasses;

namespace Gac.Logistics.Aes.Api.Model
{
    public class Aes
    {
        public string Id { get; set; }
        public long BookingId { get; set; }
        public string InstanceCode { get; set; }
        public AesHeader Header { get; set; }

        public ShipmentHeader Shipment { get; set; }

        public List<ShipmentParty> ShipmentParties { get; set; }

        public Transportation Transport { get; set; }

        public List<CommodityDetails> Commodities { get; set; }
              
    

    }
}
