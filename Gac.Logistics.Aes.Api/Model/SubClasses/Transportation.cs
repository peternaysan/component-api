using System.Collections.Generic;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class Transportation
    {
        public string ModeofTransport { get; set; }

        public string CarrierCode { get; set; }

        public string VesselName { get; set; }

        public List<TransportationDetails> TransportationDetails { get; set; }
    }



}
