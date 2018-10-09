using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Gac.Logistics.Aes.Api.Data
{
    public interface IDocumentDbRepository
    {
        Task<T> GetItemAsync<T>(string id) where T : class;

        Task<IEnumerable<T>> GetItemsAsync<T>() where T : class;

        Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        IEnumerable<T> CreateDocumentQuery<T>(string query, FeedOptions options) where T : class;

        Task<Document> CreateItemAsync<T>(T item) where T : class;

        Task<Document> CreateItemAsync<T>(T item, RequestOptions options) where T : class;

        Task<Document> UpdateItemAsync<T>(string id, T item) where T : class;

        Task<Document> UpsertItemAsync<T>(string id, T item) where T : class;

        Task<ResourceResponse<Attachment>> CreateAttachmentAsync(string attachmentsLink, object attachment, RequestOptions options);


        Task<ResourceResponse<Attachment>> ReplaceAttachmentAsync(Attachment attachment, RequestOptions options);

        Task DeleteItemAsync(string id);

    }
}
