using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Data
{
    public class AesDbRepository: DocumentDbRepositoryBase
    {
        public AesDbRepository(IConfiguration configuration):base(configuration,"aes")
        {          
            //Endpoint = configuration["AppSettings:CosmosConnectionEndPoint"];
            //Key = configuration["AppSettings:CosmosKey"];
            //DatabaseId = configuration["AppSettings:DatabaseID"];
        }

     
    }
}
