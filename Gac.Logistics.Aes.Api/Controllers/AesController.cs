using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Gac.Logistics.Aes.Api.Model;
using Gac.Logistics.Aes.Api.Data;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.SystemFunctions;

namespace Gac.Logistics.Aes.Controllers
{
    [Route("api/aes")]
    [ApiController]
    public class AesController : ControllerBase
    {
        private IDocumentDbRepository<AesDbRepository> aesDbRepository;

        public AesController(IDocumentDbRepository<AesDbRepository> aesDbRepository)
        {
            this.aesDbRepository = aesDbRepository;

        }
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
            await this.aesDbRepository.InitAsync("aes");
            var item = await this.aesDbRepository.GetItemAsync<Api.Model.Aes>(id);
            return new ObjectResult(item);
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult> Post(Api.Model.Aes aes)
        {
            await this.aesDbRepository.InitAsync("aes");
            var item = await aesDbRepository.GetItemsAsync<Api.Model.Aes>(obj=>obj.BookingId==aes.BookingId && obj.InstanceCode==aes.InstanceCode);
            Document document = null;
            if (item !=null && item.Any())
            {
                var aesObj = item.FirstOrDefault();
                document = await this.aesDbRepository.UpdateItemAsync(aesObj.Id,aesObj);

            }
            else
            {
                document = await this.aesDbRepository.CreateItemAsync(aes);

            }
            return new ObjectResult(document);
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
