using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model.Acknowledgements
{
    public class AesCustomsResponse
    {
        public Ftpcommodityshipment ftpcommodityShipment { get; set; }
    }

    public class Ftpcommodityshipment
    {
        public Ftpshipmentheader ftpshipmentHeader { get; set; }
        public Ftpshipmentheaderresponse ftpshipmentHeaderResponse { get; set; }
        public Ftpcommoditylineitemgroup ftpcommodityLineItemGroup { get; set; }
    }

    public class Ftpshipmentheader
    {
        public string shipmentReferenceNumber { get; set; }
    }

    public class Ftpshipmentheaderresponse
    {
        public string responseCode { get; set; }
        public string finalDispositionIndicator { get; set; }
        public string narrativeText { get; set; }
        public string internalTransactionNumber { get; set; }
    }

    public class Ftpcommoditylineitemgroup
    {
        public Ftplineitemheadercontinuationresponse[] ftplineItemHeaderContinuationResponse { get; set; }
    }

    public class Ftplineitemheadercontinuationresponse
    {
        public string responseCode { get; set; }
        public string severityIndicator { get; set; }
        public string narrativeText { get; set; }
        public string finalDispositionIndicator { get; set; }
        public string internalTransactionNumber { get; set; }
    }

}
