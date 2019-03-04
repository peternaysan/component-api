using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Data
{
    public class ExportInformationCodeDbRepository:DocumentDbRepositoryBase
    {
        public ExportInformationCodeDbRepository(IConfiguration configuration) : base(configuration, "exportInformationCode")
        {

        }
    }
}
