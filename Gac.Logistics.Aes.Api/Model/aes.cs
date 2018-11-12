using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model
{
    public class Aes : GetsAes
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("bookingId")]
        public string BookingId { get; set; }            

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("submittedOn")]
        public DateTime? SubmittedOn { get; set; }
        
        [JsonProperty("draftDate")]
        public string DraftDate { get; internal set; }

        [JsonProperty("pic")]
        public UserDto PicUser {get; set; }

        [JsonProperty("submittedUser")]
        public UserDto SubmittedUser { get; set; }
    }
}
