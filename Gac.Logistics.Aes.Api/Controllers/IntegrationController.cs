using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Hubs;
using Gac.Logistics.Aes.Api.Model;
using Gac.Logistics.Aes.Api.Model.Acknowledgements;
using Gac.Logistics.Aes.Api.Model.SubClasses;
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

            if (getsResponse.Ack == null)
            {
                return BadRequest("Invalid gets response object, AES is missing");
            }
            if (getsResponse.Ack.Aes == null)
            {
                return BadRequest("Invalid gets response object, AES is missing");
            }
            if (string.IsNullOrEmpty(getsResponse.Ack.Aes.ShipmentReferenceNumber))
            {
                return BadRequest("Invalid gets response object, ACK->AES->ShipmentReferenceNumber is missing");
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber == getsResponse.Ack.Aes.ShipmentReferenceNumber)
                                .Result
                                .FirstOrDefault();
            if (item == null)
            {
                return BadRequest($"Invalid shipment reference no ${getsResponse.Ack.Aes.ShipmentReferenceNumber}");
            }

            if (getsResponse.Ack.Aes.Status == GetsStatus.SUCCESS)
            {
                item.SubmissionStatus = AesStatus.GETSAPPROVED;
                item.SubmissionStatusDescription = getsResponse.Ack.Aes.StatusDescription;
                await aesDbRepository.UpdateItemAsync(item.Id, item);
            }
            else if (getsResponse.Ack.Aes.Status == GetsStatus.FAIL)
            {
                item.SubmissionStatus = AesStatus.GETSREJECTED;
                if (getsResponse.Ack.Aes.Error != null)
                {
                    item.SubmissionStatusDescription = getsResponse.Ack.Aes.Error.ErrorDescription;
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
        public async Task<ActionResult> AckAesCustomsResponse(AesCustomsResponse customsReponse)
        {
            if (customsReponse == null)
            {
                return BadRequest("Invalid gets response object");
            }

            if (customsReponse.ftpcommodityShipment == null)
            {
                return BadRequest("Invalid gets response object, ftpcommodityShipment node is missing");
            }

            if (string.IsNullOrEmpty(customsReponse.ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber))
            {
                return BadRequest("Invalid gets response object, ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber is missing");
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber == 
                                 customsReponse.ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber)
                                .Result
                                .FirstOrDefault();
           
            if (item == null)
            {
                return BadRequest($"Invalid shipment reference no ${customsReponse.ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber}");
            }
            item.SubmissionResponse = new SubmissionStatus();

            if (customsReponse.ftpcommodityShipment.ftpshipmentHeaderResponse != null)
            {
                item.SubmissionResponse.Status = "SUCCESS";
                item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                item.SubmissionStatusDescription = customsReponse.ftpcommodityShipment.ftpshipmentHeaderResponse.narrativeText;
                item.ShipmentHeader.OriginalItn = customsReponse.ftpcommodityShipment.ftpshipmentHeaderResponse.internalTransactionNumber;
                await aesDbRepository.UpdateItemAsync(item.Id, item);
            }

            await hubContext.Clients.All.SendAsync("customscallback", customsReponse);
            return Ok(true);
        }
    }
}
