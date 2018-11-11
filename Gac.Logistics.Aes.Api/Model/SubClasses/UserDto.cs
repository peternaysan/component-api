using Newtonsoft.Json;

namespace Gac.Logistics.Aes.Api.Model.SubClasses
{
    public class UserDto
    {
        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}