namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
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

        public string FppiRoutedTransport { get; set; }

        public string HazmatIndicator { get; set; }
    }
}
