using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Business.Dto;

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

        public async Task<IxErrorDto> SubmitAes(Model.GetsAes aes)
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
                        var ixErrorDto = new IxErrorDto();
                        if (response.IsSuccessStatusCode)
                        {
                            ixErrorDto.HttpStatusCode = HttpStatusCode.OK;
                            ixErrorDto.ErrorMessage = string.Empty;
                            return ixErrorDto;
                        }
                        if (response.StatusCode == HttpStatusCode.BadRequest)
                        {
                            ixErrorDto.HttpStatusCode = HttpStatusCode.BadRequest;
                            ixErrorDto.ErrorMessage = response.Content.ToString();
                            return ixErrorDto;
                        }
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            ixErrorDto.HttpStatusCode = HttpStatusCode.InternalServerError;
                            ixErrorDto.ErrorMessage = response.Content.ToString();
                            return ixErrorDto;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var err = ex.ToString();
            }
            // to do log error
            var errorDto = new IxErrorDto
                           {
                               HttpStatusCode = HttpStatusCode.InternalServerError,
                               ErrorMessage = "Ix server returned an error"
                           };
            return errorDto;
        }
    }
}
