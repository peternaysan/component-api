using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class AesHeader
    {
        [JsonProperty("acctId")]
        public string AcctId { get; set; }

        [JsonProperty("senderId")]
        public string SenderId { get; set; }

        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("actionType")]
        public string ActionType { get; set; }

        [JsonProperty("senderEmail")]
        public string SenderEmail { get; set; }
    }
}
