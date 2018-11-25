using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Business.Dto;
using Gac.Logistics.Aes.Api.Model.SubClasses;

namespace Gac.Logistics.Aes.Api.Profile
{
    public class LogisticsProfile : AutoMapper.Profile
    {
        public LogisticsProfile()
        {
            CreateMap<Model.Aes, Model.Aes>().ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Model.Aes, GfSubmissionDto>();

            CreateMap<GfSubmissionDto, Model.Aes>();
            CreateMap<GfShipmentHeader, ShipmentHeader>();
            CreateMap<GfShipmentParty, ShipmentParty>();
            CreateMap<GfTransportation, Transportation>();
        }
    } 
}
