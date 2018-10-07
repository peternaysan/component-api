using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Data
{
    public class AesDbRepository: DocumentDbRepositoryBase<AesDbRepository>,IDocumentDbRepository<AesDbRepository>
    {
        public AesDbRepository(IConfiguration configuration)
        {          
            Endpoint = configuration["AppSettings:CosmosConnectionEndPoint"];
            Key = configuration["AppSettings:CosmosKey"];
            DatabaseId = configuration["AppSettings:DatabaseID"];
        }


        public override async Task InitAsync(string collectionId)
        {
            if (client == null)
                client = new DocumentClient(new Uri(Endpoint), Key);

            if (CollectionId != collectionId)
            {
                CollectionId = collectionId;
                //collection = await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
        }

    }
}
