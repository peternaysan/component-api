using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model.Acknowledgements
{


    public class AckGetsReponse
    {
        public AckWrapper Ack { get; set; }
       
    }

    public class AckWrapper
    {
        public AesWrapper Aes { get; set; }
    }

    public class AesWrapper
    {
        public string ShipmentReferenceNumber { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public ErrorMessage Error { get; set; }
    }
  
    public class ErrorMessage
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
