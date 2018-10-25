using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Data
{
    public class CountryDbRepository : DocumentDbRepositoryBase
    {
        public CountryDbRepository(IConfiguration configuration) : base(configuration, "country")
        {
        }
    }
}
