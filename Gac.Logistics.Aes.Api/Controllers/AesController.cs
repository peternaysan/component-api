using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/aes")]
    [ApiController]
    public class AesController : ControllerBase
    {
        private readonly AesDbRepository aesDbRepository;
        private readonly IMapper mapper;

        public AesController(AesDbRepository aesDbRepository, IMapper mapper)
        {
            this.aesDbRepository = aesDbRepository;
            this.mapper = mapper;


        }
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var item = await this.aesDbRepository.GetItemAsync<Api.Model.Aes>(id);
            return new ObjectResult(item);
        }


        [HttpGet]
        [Route("getaesbybookingId")]
        public async Task<ActionResult> GetAesByBookingId(long bookingId, string instanceCode)
                
        {          
            var item = await aesDbRepository.GetItemsAsync<Api.Model.Aes>(obj => obj.BookingId == bookingId && obj.InstanceCode == instanceCode);
            return new ObjectResult(item.FirstOrDefault());
        }
        // POST api/values
        [HttpPost]
        public async Task<string> Post(Api.Model.AesExternal aes)
        {
            var item = await aesDbRepository.GetItemsAsync<Model.Aes>(obj=>obj.BookingId==aes.Aes.BookingId && obj.InstanceCode==aes.Aes.InstanceCode);
            var enumerable = item as Model.Aes[] ?? item.ToArray();
            if (enumerable.Any())
            {
                var aesObj = enumerable.FirstOrDefault();
                this.mapper.Map(aes.Aes, aesObj);
                var response = await aesDbRepository.UpdateItemAsync(aesObj.Id, aesObj);
                return response.Id;

            }
            else
            {
                var response = await aesDbRepository.CreateItemAsync(aes.Aes);
                return response.Id;

            }
            
            
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
