using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class AesHeader
    {        

        [JsonProperty("messageId")]
        public string MessageId { get; set; }

        [JsonProperty("actionType")]
        public string ActionType { get; set; }

        [JsonProperty("senderEmail")]
        public string SenderEmail { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("sentat")]
        public string Sentat { get; set; }

        [JsonProperty("senderappcode")]
        public string Senderappcode { get; set; }
    }
}
