using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Profile
{
    public class LogisticsProfile :AutoMapper.Profile
    {
        public LogisticsProfile()
        {
            CreateMap<Model.Aes, Model.Aes>().ForMember(x => x.Id, opt => opt.Ignore());
        }
    }
}
