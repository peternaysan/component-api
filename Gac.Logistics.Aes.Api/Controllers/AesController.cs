using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Gac.Logistics.Aes.Api.Model;
using Gac.Logistics.Aes.Api.Data;

namespace Gac.Logistics.Aes.Controllers
{
    [Route("api/aes")]
    [ApiController]
    public class AesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            var item = await DocumentDbRepository<Api.Model.Aes>.GetItemAsync(id);
            return new ObjectResult(item);
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult> Post(Api.Model.Aes aes)
        {
           var aesObj= await DocumentDbRepository<Api.Model.Aes>.CreateItemAsync(aes);
            return new ObjectResult(aesObj);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
