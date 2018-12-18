using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model.Acknowledgements
{
    public class AesCustomsResponse
    {
        public FtpaesResponse ftpaesResponse { get; set; }
    }

    public class FtpaesResponse
    {
        public List<FtpReponseStructure> ftpcommodityShipmentResponse { get; set; }
        public List<Ftpcommodityshipment> ftpcommodityShipment { get; set; }

    }

    public class Ftpcommodityshipment
    {
        public Ftpshipmentheader ftpshipmentHeader { get; set; }
        public List<FtpReponseStructure> ftpshipmentHeaderResponse { get; set; }
        public List<FtpReponseStructure> ftpshipmentHeaderContinuationResponse { get; set; }
        public List<FtpTransportationGroup> ftptransportationGroup { get; set; }
        public List<FtpCommodityLineitemgroup> ftpcommodityLineItemGroup { get; set; }
        public List<FtpShipmentPartyGroup> ftpshipmentPartyGroup { get; set; }
    }


    public class Ftpshipmentheader
    {
        public string shipmentReferenceNumber { get; set; }
    }

    public class FtpTransportationGroup
    {
        public List<FtpReponseStructure> transportationDetailResponse { get; set; }
    } 

    public class FtpShipmentPartyGroup
    {
        public List<FtpReponseStructure> partyHeaderResponse { get; set; }
        public List<FtpReponseStructure> partyAddressResponse { get; set; }
        public List<FtpReponseStructure> partyAddressContinuationResponse { get; set; }
    }


    public class FtpCommodityLineitemgroup
    {
        public List<FtpReponseStructure> ftplineItemHeaderResponse { get; set; }
        public List<FtpReponseStructure> ftplineItemHeaderContinuationResponse { get; set; }
        public List<FtpReponseStructure> ftpdDTCLicenseDetailResponse { get; set; }
        public List<FtpUsedVehicleGroup> ftpusedVehicleGroup { get; set; }
        public List<FtpReponseStructure> ftppgaResponse { get; set; }
        
    }

    public class FtpUsedVehicleGroup
    {
        public List<FtpUsedVehicleGroup> ftpusedVehicleDetailResponse { get; set; }
    }

    public class FtpReponseStructure
    {
        public string responseCode { get; set; }
        public string finalDispositionIndicator { get; set; }
        public string severityIndicator { get; set; }
        public string narrativeText { get; set; }
        public string internalTransactionNumber { get; set; }
        public string reasonCode { get; set; }
    }

}
