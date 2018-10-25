﻿using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Data;
using Microsoft.AspNetCore.Mvc;

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

        // POST used by GF
        [HttpPost]
        public async Task<ActionResult> Post(Api.Model.AesExternal aes)
        {
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
                var response = await aesDbRepository.CreateItemAsync(aes);
                return  Ok(new
                {
                    id = response.Id,
                    bookingId = aes.Aes.BookingId,
                    instanceCode = aes.Aes.InstanceCode
                }); 
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, [FromBody] Model.Aes aesObject)
        {
            if (aesObject == null)
            {
                return BadRequest("Invalid aes object");
            }

            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid aes id");
            }

            var item = await aesDbRepository.GetItemAsync<Model.Aes>(id);

            if (item == null)
            {
                return NotFound("Invalid aes id");
            }

            this.mapper.Map(aesObject, item);
            var response = await aesDbRepository.UpdateItemAsync(id, item);

            return Ok();
        }
    }
}
