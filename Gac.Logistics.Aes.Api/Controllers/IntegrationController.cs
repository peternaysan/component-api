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
        private readonly AesGetsResponseRepository aesGetsResponseRepository;
        private readonly IMapper mapper;
        private readonly IHubContext<AesHub> hubContext;

        public IntegrationController(AesDbRepository aesDbRepository, IMapper mapper,
                                     IHubContext<AesHub> hubContext, AesGetsResponseRepository aesGetsResponseRepository)
        {
            this.aesDbRepository = aesDbRepository;
            this.aesGetsResponseRepository = aesGetsResponseRepository;
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
            if (string.IsNullOrEmpty(getsResponse.Ack.Aes.ShipmentRefNo))
            {
                return BadRequest("Invalid gets response object, ACK->AES->ShipmentReferenceNumber is missing");
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber == getsResponse.Ack.Aes.ShipmentRefNo)
                                .Result
                                .FirstOrDefault();
            if (item == null)
            {
                return BadRequest($"Invalid shipment reference no ${getsResponse.Ack.Aes.ShipmentRefNo}");
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
                if (getsResponse.Ack.Aes.Error != null && getsResponse.Ack.Aes.Error.Count > 0)
                {
                    item.SubmissionStatusDescription = getsResponse.Ack.Aes.Error.First().ErrorDescription;
                }

                var aesGetsResponse = new AesGetsResponse {ackGetsReponse = getsResponse};
                await aesGetsResponseRepository.CreateItemAsync(aesGetsResponse);
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
            if (customsReponse.ftpaesResponse == null)
            {
                return BadRequest("Invalid gets response object,ftpaesResponse node is missing");
            }

            if (customsReponse.ftpaesResponse.ftpcommodityShipment == null)
            {
                return BadRequest("Invalid gets response object, ftpcommodityShipment node is missing");
            }
            if (customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeader == null)
            {
                return BadRequest("Invalid gets response object, ftpshipmentHeader node is missing");
            }

            if (string.IsNullOrEmpty(customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber))
            {
                return BadRequest("Invalid gets response object, ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber is missing");
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber ==
                                 customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber)
                                .Result
                                .FirstOrDefault();

            if (item == null)
            {
                return BadRequest($"Invalid shipment reference no ${customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber}");
            }

            item.SubmissionResponse = new SubmissionStatus();
            if (!string.IsNullOrEmpty(customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeaderResponse?.internalTransactionNumber))
            {
                item.SubmissionResponse.Status = "SUCCESS";
                item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                item.SubmissionStatusDescription = customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeaderResponse.narrativeText;
                item.ShipmentHeader.OriginalItn = customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeaderResponse.internalTransactionNumber;
                //further available values
                //customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeaderResponse.responseCode;
                //customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeaderResponse.severityIndicator;                

                await aesDbRepository.UpdateItemAsync(item.Id, item);
            }
            else if (customsReponse.ftpaesResponse.ftpcommodityShipment.ftpcommodityLineItemGroup?.ftplineItemHeaderContinuationResponse != null)
            {
                foreach (var line in customsReponse.ftpaesResponse.ftpcommodityShipment.ftpcommodityLineItemGroup.ftplineItemHeaderContinuationResponse)
                {
                    var responseItem = new CustomsResponse()
                    {
                        NarrativeText = line.narrativeText,
                        ResponseCode = line.responseCode,
                        SeverityIndicator = line.severityIndicator
                    };

                    item.SubmissionResponse.CustomsResponseList.Add(responseItem);

                    if (responseItem.NarrativeText.ToUpper().Contains("REJECTED"))
                    {
                        item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                        item.SubmissionStatusDescription = line.narrativeText;
                    }

                    if (!string.IsNullOrEmpty(line.internalTransactionNumber))
                    {
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.ShipmentHeader.OriginalItn = line.internalTransactionNumber;
                        item.SubmissionStatusDescription = line.narrativeText;
                    }
                    await aesDbRepository.UpdateItemAsync(item.Id, item);
                }
            }

            // signalr notoificatio
            await hubContext.Clients.All.SendAsync("customscallback", new
            {
                itn = item.ShipmentHeader.OriginalItn,
                status = item.SubmissionResponse.Status,
                description = item.SubmissionStatus,
                errorList = item.SubmissionResponse.CustomsResponseList
            });
            return Ok(true);
        }
    }
}
