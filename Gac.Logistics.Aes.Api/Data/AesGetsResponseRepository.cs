using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Data
{
    public class AesGetsResponseRepository :DocumentDbRepositoryBase
    {
        public AesGetsResponseRepository(IConfiguration configuration) : base(configuration, "aesGetsResponse")
        {
           
        }
    }
}
