﻿using System;
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
            if (id == null) // check whether id is null
            {
                return BadRequest();
            }
            var item = await this.aesDbRepository.GetItemAsync<Api.Model.Aes>(id);
            return new ObjectResult(item);
        }


        [HttpGet("getsubmissionstatus")]
        public async Task<ActionResult> GetSubmissionStatus(string id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            var item = await this.aesDbRepository.GetItemAsync<Api.Model.Aes>(id);
            return new ObjectResult(new
                                    {   item.SubmissionStatus,
                                        item.SubmittedOn,
                                        item.ShipmentHeader.ShipmentReferenceNumber
                                    });
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

            var isUltimateConsignee = false;
            foreach (var party in aes.Aes.ShipmentParty)
            {
                if (party.PartyType == "C")
                {
                    isUltimateConsignee = true;
                   break;
                }
            }

            if (!isUltimateConsignee)
            {
                return BadRequest("Invalid request object.Ultimate Consignee is missing");
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.BookingId == aes.Aes.BookingId && 
                                                                       obj.Header.Senderappcode == aes.Aes.Header.Senderappcode).Result
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

                var freightForwarder = aes.Aes.ShipmentParty.Find(x => x.PartyType == "F");
                var usppi = aesObj.ShipmentParty.Find(x => x.PartyType == "E");
                var oldUltimateConsignee = aesObj.ShipmentParty.Find(x => x.PartyType == "C");
                //properties that needs to be replaced by GF everytime, are added in GfSubmissionDto
                var gfSubmissionDto = new GfSubmissionDto();
                //map incoming object values to GfSubmissionDto and then map GfSubmissionDto to existing database object aesObj
                this.mapper.Map(aes.Aes, gfSubmissionDto);
                this.mapper.Map(gfSubmissionDto, aesObj);
                var replaceUltimateConsignee = false;
                foreach (var party in aesObj.ShipmentParty)
                {
                    if (party.PartyType == "F" && string.IsNullOrEmpty(party.StateCode))
                    {
                        party.StateCode = freightForwarder.StateCode;
                    }
                    //else if(party.PartyType == "E")
                    //{
                    //    //retain usppi statecode as its not coming from GF
                    //    party.StateCode = usppi.StateCode;
                    //}
                    else if (party.PartyType == "C")
                    {
                        replaceUltimateConsignee = oldUltimateConsignee.consigneeFromGf == "N";
                        party.consigneeFromGf = oldUltimateConsignee.consigneeFromGf;
                    }
                }

                if (replaceUltimateConsignee) // patch for keeping old ultimate consignee from GF based user choice in AES component
                {
                    var ultimateIndex = aesObj.ShipmentParty.FindIndex(party => party.PartyType == "C");
                    if(ultimateIndex >= 0)
                    {
                        aesObj.ShipmentParty.Remove(aesObj.ShipmentParty[ultimateIndex]);
                        aesObj.ShipmentParty.Add(oldUltimateConsignee);
                    }

                }               

                if (aes.Aes.CommodityDetails != null && aes.Aes.CommodityDetails.Count > 0)
                {                    
                    aesObj.CommodityDetails = aes.Aes.CommodityDetails;
                }

                var response = await aesDbRepository.UpdateItemAsync(aesObj.Id, aesObj);

                // handle direct cancellation scenario from GF , cancellation only send to IX if the shipment is already accepted from customs
                if (item.SubmissionStatus == AesStatus.CUSTOMSAPPROVED && aes.Aes.ShipmentHeader.ShipmentAction == "X") // x = cancelled
                {
                    await this.Submit(aesObj);
                }

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
            if (item.SubmissionStatus == AesStatus.CUSTOMSAPPROVED)
            {
                if (item.ShipmentHeader.ShipmentAction != "X")
                {
                    item.ShipmentHeader.ShipmentAction = "R";
                }
            }
            else if (item.ShipmentHeader.ShipmentAction != "R")
            {
                item.ShipmentHeader.ShipmentAction = "A";
            }

            item.Header.ActionType = this.Configuration["AppSettings:ActionType"];
            var getsAes = (GetsAes)item;
            getsAes.Header.Sentat = DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffZ");// DateTime.UtcNow.ToString("o");
                                                                                             //getsAes.Header.Sentat = DateTime.UtcNow.ToString("s") + "Z";
            try
            {
                var signature = await this.ixService.GetSignatureAsync(getsAes.Header.Senderappcode, getsAes.Header.Sentat);
                getsAes.Header.Signature = signature.Trim('"');
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            //if MOT=34 (Road,Other) then remove transportationDEtails node
            if (aesObject.Transportation != null && aesObject.Transportation.ModeofTransport == "34")
            {
                aesObject.Transportation.TransportationDetails = null;
            }
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

        [HttpGet("GetBySenderAppCode")]
        public async Task<ActionResult> GetBySenderAppCode(string senderAppCode)
        {
            if (senderAppCode == null)
            {
                return BadRequest();
            }

            var items = await this.aesDbRepository.GetItemsAsync<Model.Aes>(x => x.Header.Senderappcode == senderAppCode);
            
            return new ObjectResult(items.Count());
        }

        //[HttpPost("deletebysenderappcode")]
        //public async Task<ActionResult> DeleteBySenderAppcode(string senderAppCode)
        //{
        //    if (senderAppCode == null)
        //    {
        //        return BadRequest();
        //    }

        //    var items = await this.aesDbRepository.GetItemsAsync<Model.Aes>(x => x.Header.Senderappcode == senderAppCode && x.SubmittedOn < new DateTime(2019, 02, 28));            
        //    var deletedItems= new List<dynamic>();
        //    foreach (var item in items)
        //    {
        //       await this.aesDbRepository.DeleteItemAsync(item.Id);
        //        deletedItems.Add(new
        //                         {
        //                             id = item.Id,
        //                             bookingId = item.BookingId,
        //                             shipmentRefNum=item.ShipmentHeader?.ShipmentReferenceNumber,
        //                             senderappcode = item.Header?.Senderappcode
        //                         });
        //    }
        //    return new ObjectResult(deletedItems);
        //}       
    }
}



