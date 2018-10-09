using System.Linq;
using System.Threading.Tasks;
using Gac.Logistics.Aes.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/aes")]
    [ApiController]
    public class AesController : ControllerBase
    {
        private readonly IDocumentDbRepository<AesDbRepository> aesDbRepository;

        public AesController(IDocumentDbRepository<AesDbRepository> aesDbRepository)
        {
            this.aesDbRepository = aesDbRepository;

        }     
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
             aesDbRepository.Initialize("aes");
            var item = await this.aesDbRepository.GetItemAsync<Api.Model.Aes>(id);
            return new ObjectResult(item);
        }


        [HttpGet]
        [Route("getaesbybookingId")]
        public async Task<ActionResult> GetAesByBookingId(long bookingId, string instanceCode)
                
        {          
             aesDbRepository.Initialize("aes");
            var item = await aesDbRepository.GetItemsAsync<Api.Model.Aes>(obj => obj.BookingId == bookingId && obj.InstanceCode == instanceCode);
            return new ObjectResult(item.FirstOrDefault());
        }
        // POST api/values
        [HttpPost]
        public async Task<string> Post(Api.Model.Aes aes)
        {
            aesDbRepository.Initialize("aes");
            var item = await aesDbRepository.GetItemsAsync<Model.Aes>(obj=>obj.BookingId==aes.BookingId && obj.InstanceCode==aes.InstanceCode);
            var aesId = string.Empty;
            if (item !=null && item.Any())
            {
                var aesObj = item.FirstOrDefault();
                var response = await aesDbRepository.UpdateItemAsync(aesObj.Id,aes);
                aesId = response.Id;
            }
            else
            {
                var response = await aesDbRepository.CreateItemAsync(aes);

            }

            
            return document;
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
