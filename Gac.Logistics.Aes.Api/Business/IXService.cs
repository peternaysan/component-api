using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Business
{
    public class IxService
    {
        private readonly IHttpClientFactory clientFactory;
        public IConfiguration Configuration { get; }

        public IxService(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            this.clientFactory = clientFactory;
            this.Configuration = configuration;
        }

        public async Task<HttpStatusCode> SubmitAes(Model.GetsAes aes)
        {
            try
            {

                //System.Net.ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                using (var httpClientHandler = new HttpClientHandler())
                {
                    httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                    using (var client = new HttpClient(httpClientHandler))
                    {
                        // Make your request...
                        var ss = this.Configuration["AppSettings:IxEndpoint"];
                        var response = await client.PostAsJsonAsync(ss, aes); // post to base address
                        if (response.IsSuccessStatusCode)
                        {
                            return HttpStatusCode.OK;
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            return HttpStatusCode.BadRequest;
                        }
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            return HttpStatusCode.InternalServerError;
                        }
                    }
                }

                //var client = this.clientFactory.CreateClient("ix");
                //var response = await client.PostAsJsonAsync(string.Empty, aes); // post to base address
                //if (response.IsSuccessStatusCode)
                //{
                //    return true;
                //}
            }
            catch (Exception ex)
            {
                var err = ex.ToString();
            }
            // to do log error
            return HttpStatusCode.InternalServerError;
        }
    }
}
