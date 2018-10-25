using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Model.Acknowledgements;
using Microsoft.AspNetCore.Mvc;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/integration")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly AesDbRepository aesDbRepository;
        private readonly IMapper mapper;

        public IntegrationController(AesDbRepository aesDbRepository, IMapper mapper)
        {
            this.aesDbRepository = aesDbRepository;
            this.mapper = mapper;
        }

        // POST used by GF
        [HttpPost("ackgetsresponse")]
        public ActionResult AckGetsResponse(AckGetsReponse getsResponse)
        {
            return Ok(true);
        }

        [HttpPost("ackaescustomsresponse")]
        public ActionResult AckAesCustomsResponse(dynamic customsReponse)
        {
            return Ok(true);
        }
    }
}
