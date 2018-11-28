using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Business;
using Gac.Logistics.Aes.Api.Business.Dto;
using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Hubs;
using Gac.Logistics.Aes.Api.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/aes")]
    [ApiController]
    public class AesController : ControllerBase
    {
        private readonly AesDbRepository aesDbRepository;
        private readonly IxService ixService;
        private readonly IMapper mapper;
        public IConfiguration Configuration { get; }

        public AesController(AesDbRepository aesDbRepository,
                            IxService ixService,
                            IMapper mapper,
                            IConfiguration configuration)
        {
            this.aesDbRepository = aesDbRepository;
            this.ixService = ixService;
            this.mapper = mapper;
            this.Configuration = configuration;
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
            if (aes.Aes.ShipmentHeader != null && string.IsNullOrEmpty(aes.Aes.ShipmentHeader.ShipmentReferenceNumber))
            {
                return BadRequest("Invalid request object. ShipmentReferenceNumber is missing or having invalid value");
            }

            if (string.IsNullOrEmpty(aes.Aes.BookingId))
            {
                return BadRequest("Invalid request object. BookingID is missing or having invalid value");
            }
            if (aes.Aes.Header != null && string.IsNullOrEmpty(aes.Aes.Header.Senderappcode))
            {
                return BadRequest("Invalid request object.Instance Code is missing or having invalid value");
            }

            foreach (var party in aes.Aes.ShipmentParty)
            {
                if (party.PartyType == "C" && string.IsNullOrEmpty(party.PartyIdType))
                {
                    return BadRequest("Invalid request object. ID Number Type is missing for Ultimate Consignee");
                }
                if (party.PartyType == "I" && string.IsNullOrEmpty(party.PartyIdType))
                {
                    return BadRequest("Invalid request object. ID Number Type is missing for Intermediate Consignee");
                }
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.BookingId == aes.Aes.BookingId && obj.Header.Senderappcode == aes.Aes.Header.Senderappcode).Result
                                            .FirstOrDefault();


            if (item != null)
            {
                var aesObj = item;
                //check for status
                if (item.SubmissionStatus == AesStatus.SUBMITTED)
                {
                    return BadRequest("Unable to complete the Submission,Waiting for GETS Response for previous Submission");
                }
                if (item.SubmissionStatus == AesStatus.GETSAPPROVED)
                {
                    return BadRequest("Unable to complete the Submission,Waiting for Customs Response for previous Submission");
                }
                var gfSubmissionDto = new GfSubmissionDto();
                this.mapper.Map(aes.Aes, gfSubmissionDto);
                this.mapper.Map(gfSubmissionDto, aesObj);
                //this.mapper.Map(aes.Aes, aesObj);
                var response = await aesDbRepository.UpdateItemAsync(aesObj.Id, aesObj);
                return Ok(new
                {
                    id = response.Id,
                    bookingId = aes.Aes.BookingId,
                    senderappcode = aes.Aes.Header.Senderappcode
                });
            }
            else
            {
                aes.Aes.SubmissionStatus = AesStatus.PENDING;
                aes.Aes.SubmissionStatusDescription = string.Empty;
                var response = await aesDbRepository.CreateItemAsync(aes.Aes);
                return Ok(new
                {
                    id = response.Id,
                    bookingId = aes.Aes.BookingId,
                    senderappcode = aes.Aes.Header.Senderappcode
                });
            }
        }

        [HttpPost("savedraft")]
        public async Task<ActionResult> SaveDraft(Model.Aes aesObject)
        {
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
            if (item.SubmissionStatus == AesStatus.PENDING || item.SubmissionStatus == AesStatus.DRAFT)
            {
                item.SubmissionStatus = AesStatus.DRAFT;
                item.SubmissionStatusDescription = string.Empty;
            }
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
                return BadRequest("Invalid ShipmentReference Number");
            }

            if (aesObject.SubmissionStatus == AesStatus.SUBMITTED)
            {
                return BadRequest("Already submited, waiting for acknowledgement !!");
            }

            // message id
            aesObject.Header.MessageId = this.Configuration["AppSettings:MessageId"];

            aesObject.StatusNotification = new List<Model.SubClasses.StatusNotification>();
            if (!string.IsNullOrEmpty(aesObject.PicUser?.Email))
            {
                aesObject.StatusNotification.Add(new Model.SubClasses.StatusNotification()
                {
                    Name = aesObject.PicUser.FirstName,
                    NotificationType = "ALL",
                    Email = aesObject.PicUser.Email

                });
            }
            // submitted user
            if (!string.IsNullOrEmpty(aesObject.SubmittedUser?.Email))
            {
                aesObject.StatusNotification.Add(new Model.SubClasses.StatusNotification()
                {
                    Name = aesObject.SubmittedUser.FirstName,
                    NotificationType = "ALL",
                    Email = aesObject.SubmittedUser.Email

                });
            }

            this.mapper.Map(aesObject, item);
            if (item.SubmissionStatus == AesStatus.CUSTOMSREJECTED || item.SubmissionStatus == AesStatus.CUSTOMSAPPROVED)
            {
                if (item.ShipmentHeader.ShipmentAction != "X")
                {

                    item.ShipmentHeader.ShipmentAction = "R";
                }
            }
            else
            {
                item.ShipmentHeader.ShipmentAction = "A";
            }

            item.Header.ActionType = this.Configuration["AppSettings:ActionType"];
            var getsAes = (GetsAes)item;

            /*only for testing as instructed by IX team; To be changed before going to production*/
            getsAes.Header.Sentat = DateTime.UtcNow.ToString("o");
            getsAes.Header.Signature = this.ixService.GetSignatureAsync(getsAes.Header.Senderappcode, getsAes.Header.Signature, getsAes.Header.Sentat).Result;
            await aesDbRepository.UpdateItemAsync(aesObject.Id, item);
            // submit to IX
            var ixResponse = await this.ixService.SubmitAes(getsAes);
            if (ixResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                item.SubmissionStatus = AesStatus.SUBMITTED;
                item.SubmittedOn = DateTime.UtcNow;
                item.SubmissionStatusDescription = "Waiting for confirmation from GETS";
                var response = await aesDbRepository.UpdateItemAsync(aesObject.Id, item);
                return Ok(response);
            }
            if (ixResponse.HttpStatusCode == HttpStatusCode.BadRequest)
            {
                return BadRequest(ixResponse.ErrorMessage);
            }
            if (ixResponse.HttpStatusCode == HttpStatusCode.Unauthorized)
            {
                return StatusCode(401, "Authorization error when communicating with IX server");
            }
            return StatusCode(500, "An error occured while communicating with IX server");
        }

    }
}



