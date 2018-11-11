using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/aestransactions")]
    [ApiController]
    public class AesTransactionController : ControllerBase
    {
        private readonly AesTransactionDbRepository aesTransactionDbRepository;
        public AesTransactionController(AesTransactionDbRepository aesTransactionDbRepository)
        {
            this.aesTransactionDbRepository = aesTransactionDbRepository;

        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var items = await this.aesTransactionDbRepository.GetItemsAsync<Api.Model.AesTransaction>(x=>x.AesDetailEntity.Id == id);
            return new ObjectResult(items);
        }
    }
}
