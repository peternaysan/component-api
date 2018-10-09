﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;


namespace Gac.Logistics.Aes.Api.Data
{
    public class AesTransactionDbRepository:DocumentDbRepositoryBase<AesTransactionDbRepository>
    {
        public AesTransactionDbRepository(IConfiguration configuration) : base(configuration,"aesTransaction")
        {
            Endpoint = configuration["AppSettings:CosmosConnectionEndPoint"];
            Key = configuration["AppSettings:CosmosKey"];
            DatabaseId = configuration["AppSettings:DatabaseID"];
        }
      

    }
}
