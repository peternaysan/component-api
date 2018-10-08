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
        private static string Endpoint = string.Empty;
        private static string Key = string.Empty;
        private static DocumentClient client;

        public static void Initialize(IConfiguration configuration)
        {
            Endpoint = configuration["AppSettings:CosmosConnectionEndPoint"];
            Key = configuration["AppSettings:CosmosKey"];

            client = new DocumentClient(new Uri(Endpoint), Key);

        }

        private static async Task InitGalleryAsync(IConfiguration configuration)
        {
            AesDbRepository galleryRepository = new AesDbRepository(configuration);
            await galleryRepository.InitAsync("Pictures");

            var aes = await galleryRepository.GetItemsAsync<Model.Aes>();
            if (!aes.Any())
            {
            }
        }
    }
}
