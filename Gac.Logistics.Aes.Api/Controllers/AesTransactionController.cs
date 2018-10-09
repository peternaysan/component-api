using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace Gac.Logistics.Aes.Api.Controllers
{
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
            var item = await this.aesTransactionDbRepository.GetItemAsync<Api.Model.AesTransaction>(id);
            return new ObjectResult(item);
        }
    }
}
