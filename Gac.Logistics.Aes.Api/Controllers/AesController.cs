using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Business;
using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Hubs;
using Gac.Logistics.Aes.Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/aes")]
    [ApiController]
    public class AesController : ControllerBase
    {
        private readonly AesDbRepository aesDbRepository;
        private readonly IxService ixService;
        private readonly IMapper mapper;
        private readonly IHubContext<AesHub> hubContext;

        public AesController(AesDbRepository aesDbRepository,
                            IxService ixService,
                            IMapper mapper,
                            IHubContext<AesHub> hubContext)
        {
            this.aesDbRepository = aesDbRepository;
            this.ixService = ixService;
            this.mapper = mapper;
            this.hubContext = hubContext;
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

        // POST used by GF
        [HttpPost]
        public async Task<ActionResult> Post(Api.Model.AesExternal aes)
        {
            if (aes == null)
            {
                BadRequest("Invalid aes object");
            }

            if (aes.Aes == null)
            {
                BadRequest("Invalid aes object");
            }
            if (aes.Aes.ShipmentHeader !=null && string.IsNullOrEmpty(aes.Aes.ShipmentHeader.ShipmentReferenceNumber))
            {
                return BadRequest("Invalid request object. ShipmentReferenceNumber is missing or having invalid value");
            }

            if (string.IsNullOrEmpty(aes.Aes.BookingId))
            {
                return BadRequest("Invalid request object. BookingID is missing or having invalid value");
            }
            if (string.IsNullOrEmpty(aes.Aes.InstanceCode))
            {
                return BadRequest("Invalid request object.Instance Code is missing or having invalid value");
            }

            var item = await aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.BookingId == aes.Aes.BookingId && obj.InstanceCode == aes.Aes.InstanceCode);
            var enumerable = item as Model.Aes[] ?? item.ToArray();
            if (enumerable.Any())
            {
                var aesObj = enumerable.FirstOrDefault();
                this.mapper.Map(aes.Aes, aesObj);
                var response = await aesDbRepository.UpdateItemAsync(aesObj.Id, aesObj);
                return Ok(new
                {
                    id = response.Id,
                    bookingId = aes.Aes.BookingId,
                    instanceCode = aes.Aes.InstanceCode
                });
            }
            else
            {
                aes.Aes.SubmissionStatus = AesStatus.PENDING;
                var response = await aesDbRepository.CreateItemAsync(aes.Aes);
                return Ok(new
                {
                    id = response.Id,
                    bookingId = aes.Aes.BookingId,
                    instanceCode = aes.Aes.InstanceCode
                });
            }
        }

        [HttpPost("savedraft")]
        public async Task<ActionResult> SaveDraft(Model.Aes aesObject)
        {
            await hubContext.Clients.All.SendAsync("hi", "subin", "xxxxxxxxxxxxxxx");
            //await new AesHub().SendMessage("subin", "test");
            if (aesObject == null)
            {
                return BadRequest("Invalid aes object");
            }

            if (string.IsNullOrEmpty(aesObject.Id))
            {
                return BadRequest("Invalid aes id");
            }

            var item = await aesDbRepository.GetItemAsync<Model.Aes>(aesObject.Id);
            if (item == null)
            {
                return NotFound("Invalid aes id");
            }

            this.mapper.Map(aesObject, item);
            item.SubmissionStatus = AesStatus.DRAFT;
            item.DraftDate = DateTime.UtcNow.ToString();
            var response = await aesDbRepository.UpdateItemAsync(aesObject.Id, item);


            return Ok();
        }

        [HttpPost("submit")]
        public async Task<ActionResult> Submit(Model.Aes aesObject)
        {

            if (aesObject == null)
            {
                return BadRequest("Invalid aes object");
            }

            var item = await aesDbRepository.GetItemAsync<Model.Aes>(aesObject.Id);
            if (item == null)
            {
                return NotFound("Invalid aes id");
            }
            if (string.IsNullOrEmpty(aesObject.ShipmentHeader.ShipmentReferenceNumber))
            {
                return NotFound("Invalid ShipmentReference Number");
            }

            this.mapper.Map(aesObject, item);
            var response = await aesDbRepository.UpdateItemAsync(aesObject.Id, item);

            // submit to IX
            var getsAes = (GetsAes) item;
            var sucess = await this.ixService.SubmitAes(getsAes);
            if (sucess)
            {
                item.SubmissionStatus = AesStatus.SUBMITTED;
                response = await aesDbRepository.UpdateItemAsync(aesObject.Id, item);
                return Ok(response);
            }
            else
            {
                return StatusCode(500, "An error occured while communicating with IX server");
            }

        }
    }
}

    

