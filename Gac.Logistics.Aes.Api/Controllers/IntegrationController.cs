﻿using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Hubs;
using Gac.Logistics.Aes.Api.Model;
using Gac.Logistics.Aes.Api.Model.Acknowledgements;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/integration")]
    [ApiController]
    public class IntegrationController : ControllerBase
    {
        private readonly AesDbRepository aesDbRepository;
        private readonly IMapper mapper;
        private readonly IHubContext<AesHub> hubContext;

        public IntegrationController(AesDbRepository aesDbRepository, IMapper mapper,
                                     IHubContext<AesHub> hubContext)
        {
            this.aesDbRepository = aesDbRepository;
            this.mapper = mapper;
            this.hubContext = hubContext;
        }

        // POST used by GF
        [HttpPost("ackgetsresponse")]
        public async Task<ActionResult> AckGetsResponse(AckGetsReponse getsResponse)
        {
            if (getsResponse == null)
            {
                return BadRequest("Invalid gets response object");
            }

            if (getsResponse.ACK == null)
            {
                return BadRequest("Invalid gets response object, ACK is missing");
            }

            if (string.IsNullOrEmpty(getsResponse.ACK.ShipmentReferenceNumber))
            {
                return BadRequest("Invalid gets response object, ACK->ShipmentReferenceNumber is missing");
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber == getsResponse.ACK.ShipmentReferenceNumber)
                                .Result
                                .FirstOrDefault();
            if (item == null)
            {
                return BadRequest($"Invalid shipment reference no ${getsResponse.ACK.ShipmentReferenceNumber}");
            }

            if (getsResponse.ACK.Status == GetsStatus.SUCCESS)
            {
                item.SubmissionStatus = AesStatus.GETSAPPROVED;
                item.SubmissionStatusDescription = getsResponse.ACK.StatusDescription;
                await aesDbRepository.UpdateItemAsync(item.Id, item);
            }
            else if (getsResponse.ACK.Status == GetsStatus.FAIL)
            {
                item.SubmissionStatus = AesStatus.GETSREJECTED;
                if (getsResponse.Error != null)
                {
                    item.SubmissionStatusDescription = getsResponse.Error.ErrorDescription;
                }
                await aesDbRepository.UpdateItemAsync(item.Id, item);
            }
            else
            {
                return BadRequest("Invalid status value");
            }

            await hubContext.Clients.All.SendAsync("getscallback", getsResponse);
            return Ok(true);
        }

        [HttpPost("ackaescustomsresponse")]
        public ActionResult AckAesCustomsResponse(dynamic customsReponse)
        {
            return Ok(true);
        }
    }
}
