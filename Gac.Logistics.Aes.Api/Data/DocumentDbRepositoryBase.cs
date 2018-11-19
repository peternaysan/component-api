using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;

namespace Gac.Logistics.Aes.Api.Data
{
    public abstract class DocumentDbRepositoryBase : IDocumentDbRepository
    {
        protected string Endpoint;
        protected string Key;
        protected string DatabaseId;
        protected string CollectionId;
        protected DocumentClient Client;
        protected DocumentCollection Collection;

        protected DocumentDbRepositoryBase(IConfiguration configuration, string collectionId)
        {
            Endpoint = configuration["AppSettings:CosmosConnectionEndPoint"];
            Key = configuration["AppSettings:CosmosKey"];
            DatabaseId = configuration["AppSettings:DatabaseID"];
            this.CollectionId = collectionId;
            if (Client == null)
                Client = new DocumentClient(new Uri(Endpoint), Key);

        }

        public async Task<T> GetItemAsync<T>(string id) where T : class
        {
            try
            {
                Document document =
                    await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>() where T : class
        {
            IDocumentQuery<T> query = Client.CreateDocumentQuery<T>(
                                                                    UriFactory.CreateDocumentCollectionUri(DatabaseId,
                                                                                                           CollectionId),
                                                                    new FeedOptions
                                                                    {
                                                                        MaxItemCount = -1,
                                                                        EnableCrossPartitionQuery = true
                                                                    })
                                            .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public async Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            IDocumentQuery<T> query = Client.CreateDocumentQuery<T>(
                                                                    UriFactory.CreateDocumentCollectionUri(DatabaseId,
                                                                                                           CollectionId),
                                                                    new FeedOptions
                                                                    {
                                                                        MaxItemCount = -1,
                                                                        EnableCrossPartitionQuery = true
                                                                    })
                                            .Where(predicate)
                                            .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }



            return results;
        }
        public int FindItemsCount<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var count = Client.CreateDocumentQuery<T>(
                                                                    UriFactory.CreateDocumentCollectionUri(DatabaseId,
                                                                                                           CollectionId),
                                                                    new FeedOptions
                                                                    {
                                                                        MaxItemCount = -1,
                                                                        EnableCrossPartitionQuery = true
                                                                    })
                                            .Where(predicate)
                                            .Count();
            return count;
        }

        public IEnumerable<T> CreateDocumentQuery<T>(string query, FeedOptions options) where T : class
        {
            return Client.CreateDocumentQuery<T>(Collection.DocumentsLink, query, options).AsEnumerable();
        }

        public async Task<Document> CreateItemAsync<T>(T item) where T : class
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId);
            return await Client.CreateDocumentAsync(collectionUri, item);
        }

        public async Task<Document> CreateItemAsync<T>(T item, RequestOptions options) where T : class
        {
            return await Client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                                                    item,
                                                    options);
        }

        public async Task<Document> UpdateItemAsync<T>(string id, T item) where T : class
        {
            return await Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item);
        }

        public async Task<Document> UpsertItemAsync<T>(string id, T item) where T : class
        {
            return await Client.UpsertDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id), item);
        }

        public async Task<ResourceResponse<Attachment>> CreateAttachmentAsync(
            string attachmentsLink,
            object attachment,
            RequestOptions options)
        {
            return await Client.CreateAttachmentAsync(attachmentsLink, attachment, options);
        }

        public async Task<ResourceResponse<Attachment>> ReplaceAttachmentAsync(
            Attachment attachment,
            RequestOptions options)
        {
            return await Client.ReplaceAttachmentAsync(attachment, options);
        }

        public async Task DeleteItemAsync(string id)
        {
            await Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id));
        }


        public async Task<StoredProcedureResponse<dynamic>> ExecuteStoredProcedureAsync(
            string procedureName,
            string query,
            string partitionKey)
        {
            StoredProcedure storedProcedure = Client.CreateStoredProcedureQuery(Collection.StoredProceduresLink)
                                                    .Where(sp => sp.Id == procedureName)
                                                    .AsEnumerable()
                                                    .FirstOrDefault();

            return await Client.ExecuteStoredProcedureAsync<dynamic>(storedProcedure.SelfLink,
                                                                     new RequestOptions
                                                                     {
                                                                         PartitionKey =
                                                                             new
                                                                                 PartitionKey(partitionKey)
                                                                     },
                                                                     query);

        }


        public async Task CreateDatabaseIfNotExists()
        {
            try
            {
                await Client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await Client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task CreateCollectionIfNotExists(string partitionkey = null)
        {
            try
            {
                await Client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    if (string.IsNullOrEmpty(partitionkey))
                    {
                        await Client.CreateDocumentCollectionAsync(
                            UriFactory.CreateDatabaseUri(DatabaseId),
                            new DocumentCollection { Id = CollectionId },
                            new RequestOptions { OfferThroughput = 1000 });
                    }
                    else
                    {
                        await Client.CreateDocumentCollectionAsync(
                            UriFactory.CreateDatabaseUri(DatabaseId),
                            new DocumentCollection
                            {
                                Id = CollectionId,
                                PartitionKey = new PartitionKeyDefinition
                                {
                                    Paths = new Collection<string> { "/" + partitionkey }
                                }
                            },
                            new RequestOptions { OfferThroughput = 1000 });
                    }

                }
                else
                {
                    throw;
                }
            }
        }



    }


}
