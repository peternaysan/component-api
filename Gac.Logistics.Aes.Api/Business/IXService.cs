using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Business
{
    public class IxService
    {
        private readonly IHttpClientFactory clientFactory;
        public IxService(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        public async Task<bool> SubmitAes(Model.Aes aes)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "");
            var client = this.clientFactory.CreateClient("ix");
            var response = await client.PostAsJsonAsync(string.Empty, aes); // post to base address
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
