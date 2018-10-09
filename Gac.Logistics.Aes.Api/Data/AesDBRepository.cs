using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Data
{
    public class AesDbRepository: DocumentDbRepositoryBase<AesDbRepository>
    {
        public AesDbRepository(IConfiguration configuration)
        {          
            Endpoint = configuration["AppSettings:CosmosConnectionEndPoint"];
            Key = configuration["AppSettings:CosmosKey"];
            DatabaseId = configuration["AppSettings:DatabaseID"];
        }


        public override void Initialize(string collectionId)
        {
            if (Client == null)
                Client = new DocumentClient(new Uri(Endpoint), Key);

            if (CollectionId != null && CollectionId != collectionId)
            {
                CollectionId = collectionId;
            }
        }

        //private static async Task CreateCollectionIfNotExistsAsync()
        //{
        //    try
        //    {
        //        if (Client == null)
        //            Client = new DocumentClient(new Uri(Endpoint), Key);
        //        await Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
        //    }
        //    catch (DocumentClientException e)
        //    {
        //        if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        //        {
        //            await Client.CreateDocumentCollectionAsync(
        //                                                       UriFactory.CreateDatabaseUri(DatabaseId),
        //                                                       new DocumentCollection { Id = CollectionId },
        //                                                       new RequestOptions { OfferThroughput = 1000 });
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}
    }
}
