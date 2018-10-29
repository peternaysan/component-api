using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class CommodityLineDetails
    {
        [JsonProperty("productId")]
        public string ProductId { get; set; }

        [JsonProperty("commodityAction")]
        public string CommodityAction { get; set; }

        [JsonProperty("exportInformationCode")]
        public string ExportInformationCode { get; set; }

        [JsonProperty("commodityDescription")]
        public string CommodityDescription { get; set; }

        [JsonProperty("htsNumber")]
        public string HTSNumber { get; set; }

        [JsonProperty("quantity1")]
        public string Quantity1 { get; set; }

        [JsonProperty("quantity1Uom")]
        public string Quantity1UOM { get; set; }

        [JsonProperty("valueofGoods")]
        public string ValueofGoods { get; set; }

        [JsonProperty("shippingWeight")]
        public string ShippingWeight { get; set; }

        [JsonProperty("originofGoods")]
        public string OriginofGoods { get; set; }
    }
}
