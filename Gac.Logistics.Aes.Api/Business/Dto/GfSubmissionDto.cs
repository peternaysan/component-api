using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Business.Dto
{
    public class GfSubmissionDto
    {       

        [JsonProperty("bookingId")]
        public string BookingId { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("submittedOn")]
        public DateTime? SubmittedOn { get; set; }       

        [JsonProperty("pic")]
        public UserDto PicUser { get; set; }

        [JsonProperty("submittedUser")]
        public UserDto SubmittedUser { get; set; }

        [JsonProperty("shipmentHeader")]
        public GfShipmentHeader ShipmentHeader { get; set; }

        [JsonProperty("shipmentParty")]
        public List<GfShipmentParty> ShipmentParty { get; set; }

        //[JsonProperty("commodityDetails")]
        //public List<GfCommodityDetails> CommodityDetails { get; set; }

        [JsonProperty("transportation")]
        public GfTransportation Transportation { get; set; }
    }

    public class GfShipmentHeader
    {
        [JsonProperty("shipmentReferenceNumber")]
        public string ShipmentReferenceNumber { get; set; }

        [JsonProperty("estimatedExportDate")]
        public string EstimatedExportDate { get; set; }

        [JsonProperty("portofExportation")]
        public string PortofExportation { get; set; }

        [JsonProperty("portofUnlading")]
        public string PortofUnlading { get; set; }

        [JsonProperty("inbondCode")]
        public string InbondCode { get; set; }

        [JsonProperty("ultimateDestinationCountry")]
        public string UltimateDestinationCountry { get; set; }


    }

    public class GfShipmentParty
    {
        [JsonProperty("partyId")]
        public string PartyId { get; set; }

        [JsonProperty("partyIdType")]
        public string PartyIdType { get; set; }

        [JsonProperty("partyType")]
        public string PartyType { get; set; }
        [JsonProperty("partyName")]
        public string PartyName { get; set; }

        [JsonProperty("contactFirstName")]
        public string ContactFirstName { get; set; }

        [JsonProperty("contactLastName")]
        public string ContactLastName { get; set; }

        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }
    }

    public class GfCommodityDetails
    {
        [JsonProperty("commodityLineDetails")]
        public GfCommodityLineDetails CommodityLineDetails { get; set; }

        [JsonProperty("licenseDetails")]
        public GfCommodityLicenseDetails LicenseDetails { get; set; }
    }

    public class GfCommodityLineDetails
    {
        [JsonProperty("exportInformationCode")]
        public string ExportInformationCode { get; set; }

        [JsonProperty("htsNumber")]
        public string HTSNumber { get; set; }

        [JsonProperty("commodityDescription")]
        public string CommodityDescription { get; set; }

        [JsonProperty("valueofGoods")]
        public string ValueofGoods { get; set; }
    }

    public class GfCommodityLicenseDetails
    {
        [JsonProperty("eccn")]
        public string Eccn { get; set; }
    }

    public class GfTransportation
    {
        [JsonProperty("modeofTransport")]
        public string ModeofTransport { get; set; }

        [JsonProperty("carrierCode")]
        public string CarrierCode { get; set; }

        [JsonProperty("vesselName")]
        public string VesselName { get; set; }

        [JsonProperty("transportationDetails")]
        public List<GfTransportationDetails> TransportationDetails { get; set; }
    }

    public class GfTransportationDetails
    {
        [JsonProperty("equipmentNumber")]

        public string EquipmentNumber { get; set; }

        [JsonProperty("sealNumber")]

        public string SealNumber { get; set; }

        [JsonProperty("transportationReferenceNumber")]

        public string TransportationReferenceNumber { get; set; }
    }
}
