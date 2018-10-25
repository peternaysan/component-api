using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model.Acknowledgements
{
    public class AckCustomsReponse
    {
        public Ack ACK { get; set; }
        public ErrorMessage Error { get; set; }
    }

    public class AckGetsReponse
    {
        public Ack ACK { get; set; }
        public ErrorMessage Error { get; set; }
    }

    public class Ack
    {
        public string ShipmentReferenceNumber { get; set; }
        public string Status { get; set; }
    }

    public class ErrorMessage
    {
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }
}
