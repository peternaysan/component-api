using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model
{
    public class Aes
    {
        public string Id { get; set; }
        public header Header { get; set; }

        public ShipmentHeader Shipment { get; set; }

        public List<ShipmentParty> ShipmentParties { get; set; }

        public Transportation Transport { get; set; }

        public List<CommodityDetails> Commodities { get; set; }

        public class CommodityDetails
        {
            public CommodityDetailsCommodityLineDetails[] CommodityLineDetails { get; set; }

            public CommodityDetailsLicenseDetails[] LicenseDetails { get; set; }
        }

        public class CommodityDetailsCommodityLineDetails
        {
            public string productId { get; set; }

            public string CommodityAction { get; set; }

            public string ExportInformationCode { get; set; }

            public string CommodityDescription { get; set; }

            public string HTSNumber { get; set; }

            public string Quantity1 { get; set; }

            public string Quantity1UOM { get; set; }

            public string ValueofGoods { get; set; }

            public string ShippingWeight { get; set; }
        }

        public class CommodityDetailsLicenseDetails
        {
            public string LicenseExemptionCode { get; set; }

            public string ExportLicenseNumber { get; set; }
        }

        public class ShipmentHeader
        {
            public string RelatedParties { get; set; }

            public string FilingTypeIndicator { get; set; }

            public string ShipmentAction { get; set; }

            public string ShipmentReferenceNumber { get; set; }

            public string EstimatedExportDate { get; set; }

            public string PortofExportation { get; set; }

            public string PortofUnlading { get; set; }

            public string InbondCode { get; set; }

            public string EntryNumber { get; set; }

            public string ForeignTradeZone { get; set; }

            public string OriginState { get; set; }

            public string UltimateDestinationCountry { get; set; }

            public string FPPIRoutedTransport { get; set; }

            public string HAZMATIndicator { get; set; }
        }

        public class ShipmentParty
        {
            public string PartyID { get; set; }

            public string PartyIDType { get; set; }

            public string PartyType { get; set; }

            public string PartyName { get; set; }

            public string ContactFirstName { get; set; }

            public string ContactLastName { get; set; }

            public string AddressLine1 { get; set; }

            public string ContactPhoneNumber { get; set; }

            public string City { get; set; }

            public string StateCode { get; set; }

            public string CountryCode { get; set; }

            public string UltimateConsigneeType { get; set; }

            public string PostalCode { get; set; }

            public string USPPIIRSNumber { get; set; }

            public string USPPIIRSIDType { get; set; }
        }

        public class Transportation
        {
            public string ModeofTransport { get; set; }

            public string CarrierCode { get; set; }

            public string VesselName { get; set; }

            public List<TransportationTransportationDetails> TransportationDetails { get; set; }
        }

        public class TransportationTransportationDetails
        {
            public string EquipmentNumber { get; set; }

            public string TransportationReferenceNumber { get; set; }
        }

        public class header
        {
            public string acctID { get; set; }

            public string senderID { get; set; }

            public string messageID { get; set; }

            public string actionType { get; set; }

            public string senderEmail { get; set; }
        }

    }
}
