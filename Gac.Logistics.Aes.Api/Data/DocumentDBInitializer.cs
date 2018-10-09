using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Data
{
    public class DocumentDbInitializer
    {
        private static string endpoint = string.Empty;
        private static string key = string.Empty;
        public static DocumentClient Client { get; private set; }

        public static void Initialize(IConfiguration configuration)
        {
            endpoint = configuration["AppSettings:CosmosConnectionEndPoint"];
            key = configuration["AppSettings:CosmosKey"];
            Client = new DocumentClient(new Uri(endpoint), key);

        }     
    }
}
