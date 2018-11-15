using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class StatusNotification
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("notificationType")]
        public string NotificationType { get; set; }
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }

    }
}
