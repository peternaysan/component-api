using System;
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
                item.GetsResponse = getsResponse;
                await aesDbRepository.UpdateItemAsync(item.Id, item);
            }
            else if (getsResponse.Ack.Aes.Status == GetsStatus.FAIL)
            {
                item.SubmissionStatus = AesStatus.GETSREJECTED;
                if (getsResponse.Ack.Aes.Error != null && getsResponse.Ack.Aes.Error.Count > 0)
                {
                    item.SubmissionStatusDescription = getsResponse.Ack.Aes.Error.First().ErrorDescription;
                }
                item.GetsResponse = getsResponse;
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
            var ftpcommodityShipment = customsReponse.ftpaesResponse.ftpcommodityShipment;
            if (ftpcommodityShipment.ftpshipmentHeader == null)
            {
                return BadRequest("Invalid gets response object, ftpshipmentHeader node is missing");
            }

            if (string.IsNullOrEmpty(ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber))
            {
                return BadRequest("Invalid gets response object, ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber is missing");
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber ==
                                 customsReponse.ftpaesResponse.ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber)
                                .Result
                                .FirstOrDefault();

            if (item == null)
            {
                return BadRequest($"Invalid shipment reference no ${ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber}");
            }

            item.SubmissionResponse = new SubmissionResponse();

            if (ftpcommodityShipment.ftpshipmentHeaderResponse?.Count > 0)
            {
                foreach (var shipmentHeaderResponse in ftpcommodityShipment.ftpshipmentHeaderResponse)
                {
                    item = AddItemToCustomsResponse(shipmentHeaderResponse, item);
                    if (!String.IsNullOrEmpty(shipmentHeaderResponse.internalTransactionNumber))
                    {
                        item.SubmissionResponse.Status = "SUCCESS";
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.SubmissionStatusDescription = shipmentHeaderResponse.narrativeText;
                        item.ShipmentHeader.OriginalItn = shipmentHeaderResponse.internalTransactionNumber;
                    }
                }
            }
            if (ftpcommodityShipment.ftpcommodityLineItemGroup != null)
            {
                foreach (var commodityLineItemGroup in ftpcommodityShipment.ftpcommodityLineItemGroup)
                {
                    foreach (var lineItemHeaderResponse in commodityLineItemGroup.ftplineItemHeaderResponse)
                    {
                        item = AddItemToCustomsResponse(lineItemHeaderResponse, item);

                        if (lineItemHeaderResponse.narrativeText.ToUpper().Contains("REJECTED"))
                        {
                            item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                            item.SubmissionStatusDescription = lineItemHeaderResponse.narrativeText;
                        }

                        if (string.IsNullOrEmpty(lineItemHeaderResponse.internalTransactionNumber))
                            continue;
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.ShipmentHeader.OriginalItn = lineItemHeaderResponse.internalTransactionNumber;
                        item.SubmissionStatusDescription = lineItemHeaderResponse.narrativeText;
                    }

                    foreach (var lineItemHeaderContinuationResponse in commodityLineItemGroup.ftplineItemHeaderContinuationResponse)
                    {
                        item = AddItemToCustomsResponse(lineItemHeaderContinuationResponse, item);
                        if (string.IsNullOrEmpty(lineItemHeaderContinuationResponse.internalTransactionNumber))
                            continue;
                        item.SubmissionResponse.Status = "SUCCESS";
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.SubmissionStatusDescription = lineItemHeaderContinuationResponse.narrativeText;
                        item.ShipmentHeader.OriginalItn = lineItemHeaderContinuationResponse.internalTransactionNumber;
                    }

                    foreach (var licenseDetailResponse in commodityLineItemGroup.ftpdDTCLicenseDetailResponse)
                    {
                        item = AddItemToCustomsResponse(licenseDetailResponse, item);
                        if (string.IsNullOrEmpty(licenseDetailResponse.internalTransactionNumber))
                            continue;
                        item.SubmissionResponse.Status = "SUCCESS";
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.SubmissionStatusDescription = licenseDetailResponse.narrativeText;
                        item.ShipmentHeader.OriginalItn = licenseDetailResponse.internalTransactionNumber;
                    }

                }
            }

            if (ftpcommodityShipment.ftpshipmentHeaderContinuationResponse != null)
            {


                foreach (var shipmentHeaderContinuationResponse in ftpcommodityShipment
                    .ftpshipmentHeaderContinuationResponse)
                {
                    item = AddItemToCustomsResponse(shipmentHeaderContinuationResponse, item);
                    if (String.IsNullOrEmpty(shipmentHeaderContinuationResponse.internalTransactionNumber))
                        continue;
                    item.SubmissionResponse.Status = "SUCCESS";
                    item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                    item.SubmissionStatusDescription = shipmentHeaderContinuationResponse.narrativeText;
                    item.ShipmentHeader.OriginalItn = shipmentHeaderContinuationResponse.internalTransactionNumber;
                }

            }
            if (ftpcommodityShipment.ftptransportationGroup != null)
            {
                foreach (var transportationGroup in ftpcommodityShipment.ftptransportationGroup)
                {
                    foreach (var line in transportationGroup.transportationDetailResponse)
                    {
                        item = AddItemToCustomsResponse(line, item);
                        if (String.IsNullOrEmpty(line.internalTransactionNumber))
                            continue;
                        item.SubmissionResponse.Status = "SUCCESS";
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.SubmissionStatusDescription = line.narrativeText;
                        item.ShipmentHeader.OriginalItn = line.internalTransactionNumber;
                    }
                }
            }

            if (ftpcommodityShipment.ftpshipmentPartyGroup != null)
            {
                foreach (var shipmentPartyGroup in ftpcommodityShipment.ftpshipmentPartyGroup)
                {
                    foreach (var partyHeaderResponse in shipmentPartyGroup.partyHeaderResponse)
                    {
                        item = AddItemToCustomsResponse(partyHeaderResponse, item);
                        if (String.IsNullOrEmpty(partyHeaderResponse.internalTransactionNumber))
                            continue;
                        item.SubmissionResponse.Status = "SUCCESS";
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.SubmissionStatusDescription = partyHeaderResponse.narrativeText;
                        item.ShipmentHeader.OriginalItn = partyHeaderResponse.internalTransactionNumber;
                    }
                    foreach (var partyAddressResponse in shipmentPartyGroup.partyAddressResponse)
                    {
                        item = AddItemToCustomsResponse(partyAddressResponse, item);
                        if (String.IsNullOrEmpty(partyAddressResponse.internalTransactionNumber))
                            continue;
                        item.SubmissionResponse.Status = "SUCCESS";
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.SubmissionStatusDescription = partyAddressResponse.narrativeText;
                        item.ShipmentHeader.OriginalItn = partyAddressResponse.internalTransactionNumber;
                    }
                    foreach (var partyAddressContinuationResponse in shipmentPartyGroup.partyAddressContinuationResponse)
                    {
                        item = AddItemToCustomsResponse(partyAddressContinuationResponse, item);
                        if (String.IsNullOrEmpty(partyAddressContinuationResponse.internalTransactionNumber))
                            continue;
                        item.SubmissionResponse.Status = "SUCCESS";
                        item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
                        item.SubmissionStatusDescription = partyAddressContinuationResponse.narrativeText;
                        item.ShipmentHeader.OriginalItn = partyAddressContinuationResponse.internalTransactionNumber;
                    }
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

        private Model.Aes AddItemToCustomsResponse(FtpReponseStructure response,Model.Aes item)
        {
            var responseItem = new CustomsResponse()
                               {
                                   NarrativeText = response.narrativeText,
                                   ResponseCode = response.responseCode,
                                   SeverityIndicator = response.severityIndicator,
                                   ReasonCode = response.reasonCode,
                                   FinalDestinationIndicator = response.finalDispositionIndicator
                               };

            item.SubmissionResponse.CustomsResponseList.Add(responseItem);
            return item;
        }
    }
}
