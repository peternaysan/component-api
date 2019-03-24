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
    [CustomAuthorize]
    public class IntegrationController : ControllerBase
    {
        private readonly AesDbRepository aesDbRepository;
        private readonly AesTransactionDbRepository aesTransactionDbRepository;
        private readonly IMapper mapper;
        private readonly IHubContext<AesHub> hubContext;

        public IntegrationController(AesDbRepository aesDbRepository, IMapper mapper,
                                     IHubContext<AesHub> hubContext, AesTransactionDbRepository aesTransactionDbRepository)
        {
            this.aesDbRepository = aesDbRepository;
            this.aesTransactionDbRepository = aesTransactionDbRepository;
            this.mapper = mapper;
            this.hubContext = hubContext;
        }

        // POST used by GF
        [HttpPost("ackgetsresponse")]
        public async Task<ActionResult> AckGetsResponse(AckGetsReponse getsResponse)
        {
            var telemetry = new Microsoft.ApplicationInsights.TelemetryClient();
            if (getsResponse == null)
            {
                var message = "Invalid gets response object, ackgetsresponse payload shoud not be null";
                telemetry.TrackTrace(message);
                return BadRequest(message);
            }

            if (getsResponse.Ack == null)
            {
                var message = "Invalid gets response object, getsResponse.Ack is missing in ackgetsresponse payload";
                telemetry.TrackTrace(message);
                return BadRequest(message);
            }
            if (getsResponse.Senderappcode == null)
            {
                var message = "Invalid gets response object, getsResponse.Senderappcode is missing in ackgetsresponse";
                telemetry.TrackTrace(message);
                return BadRequest(message);
            }
            if (getsResponse.Ack.Aes == null)
            {
                var message = "Invalid gets response object, getsResponse.Ack.Aes is missing in ackgetsresponse";
                telemetry.TrackTrace(message);
                return BadRequest(message);
            }
            if (string.IsNullOrEmpty(getsResponse.Ack.Aes.ShipmentRefNo))
            {
                var message = "Invalid gets response object, ACK->AES->ShipmentReferenceNumber is missing in ackgetsresponse";
                telemetry.TrackTrace(message);
                return BadRequest(message);
            }

            var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber == getsResponse.Ack.Aes.ShipmentRefNo && obj.Header.Senderappcode==getsResponse.Senderappcode)
                                .Result
                                .FirstOrDefault();
            if (item == null)
            {
                telemetry.TrackTrace($"Invalid shipment reference no ${getsResponse.Ack.Aes.ShipmentRefNo} and sender App code ${getsResponse.Senderappcode}");
                return Ok();
            }

            if (getsResponse.Ack.Aes.Status == GetsStatus.SUCCESS)
            {
                item.SubmissionStatus = AesStatus.GETSAPPROVED;
                item.SubmissionStatusDescription = getsResponse.Ack.Aes.StatusDescription;
                var getsRes = getsResponse;
                item.GetsResponse = getsRes;
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
            else if (getsResponse.Ack.Aes.Status == GetsStatus.SUBMITTED)
            {
                // ignore in AES, submition response intended for GF
                return Ok(true);
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
            if (customsReponse.senderappcode == null)
            {
                return BadRequest("Invalid gets response object,sender app code is missing");
            }

            if (customsReponse.ftpaesResponse.ftpcommodityShipment == null)
            {
                return BadRequest("Invalid gets response object, ftpcommodityShipment node is missing");
            }            

            foreach (var ftpcommodityShipment in customsReponse.ftpaesResponse.ftpcommodityShipment)
            {
                if (ftpcommodityShipment.ftpshipmentHeader == null)
                {
                    return BadRequest("Invalid gets response object, ftpshipmentHeader node is missing");
                }

                if (string.IsNullOrEmpty(ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber))
                {
                    return BadRequest("Invalid gets response object, ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber is missing");
                }
                var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber == ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber &&
                                                                            obj.Header.Senderappcode== customsReponse.senderappcode)
                                          .Result
                                          .FirstOrDefault();
                if (item == null)
                {
                    return BadRequest($"Invalid shipment reference no ${ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber} and senderappcode ${customsReponse.senderappcode}");
                }

                // important function go and see inside
                ProcessCustomsStructure(ftpcommodityShipment, item);

                await aesDbRepository.UpdateItemAsync(item.Id, item);

                //add it to transaction history
                var aesTransaction = new AesTransaction
                {
                    AesDetailEntity = item,
                    CreatedOn = DateTime.UtcNow,
                    Message = item.SubmissionStatusDescription,
                    Status = item.SubmissionStatus
                };
                await aesTransactionDbRepository.CreateItemAsync(aesTransaction);


                // signalr notoification
                await hubContext.Clients.All.SendAsync("customscallback", new
                {
                    itn = item.ShipmentHeader.OriginalItn,
                    status = item.SubmissionStatus,
                    description = item.SubmissionStatusDescription,
                    errorList = item.SubmissionResponse.CustomsResponseList,
                    shipmentRefNo=item.ShipmentHeader.ShipmentReferenceNumber,
                    senderappcode=item.Header.Senderappcode
                });


            }

            return Ok(true);
        }

        private void ProcessCustomsStructure(Ftpcommodityshipment ftpcommodityShipment, Model.Aes item)
        {
            var isShipmentCancelled = false;
            var responseText = string.Empty;
            var isShipmentRejected = false;
            
            item.SubmissionResponse = new SubmissionResponse();
            if (ftpcommodityShipment.ftpshipmentHeaderResponse?.Count > 0)
            {
                foreach (var shipmentHeaderResponse in ftpcommodityShipment.ftpshipmentHeaderResponse)
                {
                    AddItemToCustomsResponse(shipmentHeaderResponse, item);
                    if (!string.IsNullOrEmpty(shipmentHeaderResponse.internalTransactionNumber))
                    {
                        ApplyCustomsSuccessStatus(item, shipmentHeaderResponse);
                    }
                    if (shipmentHeaderResponse.narrativeText.ToUpper().Contains("REJECTED"))
                    {                      
                        isShipmentRejected = true;
                        responseText = shipmentHeaderResponse.narrativeText;
                    }
                    if (shipmentHeaderResponse.narrativeText.ToUpper().Contains("CANCELLED"))
                    {
                        isShipmentCancelled = true;
                        responseText = shipmentHeaderResponse.narrativeText;
                    }
                }
            }
            if (ftpcommodityShipment.ftpcommodityLineItemGroup != null)
            {
                foreach (var commodityLineItemGroup in ftpcommodityShipment.ftpcommodityLineItemGroup)
                {
                    if (commodityLineItemGroup.ftplineItemHeaderResponse != null)
                    {
                        foreach (var lineItemHeaderResponse in commodityLineItemGroup.ftplineItemHeaderResponse)
                        {
                            AddItemToCustomsResponse(lineItemHeaderResponse, item);

                            if (!string.IsNullOrEmpty(lineItemHeaderResponse.internalTransactionNumber))
                            {
                                ApplyCustomsSuccessStatus(item, lineItemHeaderResponse);
                            }
                            if (lineItemHeaderResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                isShipmentRejected = true;
                                responseText = lineItemHeaderResponse.narrativeText;
                            }
                        }
                    }

                    if (commodityLineItemGroup.ftplineItemHeaderContinuationResponse != null)
                    {
                        foreach (var lineItemHeaderContinuationResponse in commodityLineItemGroup.ftplineItemHeaderContinuationResponse)
                        {
                            AddItemToCustomsResponse(lineItemHeaderContinuationResponse, item);
                            if (!string.IsNullOrEmpty(lineItemHeaderContinuationResponse.internalTransactionNumber))
                            {
                                ApplyCustomsSuccessStatus(item, lineItemHeaderContinuationResponse);

                            }
                            if (lineItemHeaderContinuationResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                isShipmentRejected = true;
                                responseText = lineItemHeaderContinuationResponse.narrativeText;
                            }
                        }
                    }

                    if (commodityLineItemGroup.ftpdDTCLicenseDetailResponse != null)
                    {
                        foreach (var licenseDetailResponse in commodityLineItemGroup.ftpdDTCLicenseDetailResponse)
                        {
                            AddItemToCustomsResponse(licenseDetailResponse, item);
                            if (!string.IsNullOrEmpty(licenseDetailResponse.internalTransactionNumber))
                            {
                                ApplyCustomsSuccessStatus(item, licenseDetailResponse);

                            }
                            if (licenseDetailResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                isShipmentRejected = true;
                                responseText = licenseDetailResponse.narrativeText;
                            }
                        }
                    }
                }
            }

            if (ftpcommodityShipment.ftpshipmentHeaderContinuationResponse != null)
            {
                foreach (var shipmentHeaderContinuationResponse in ftpcommodityShipment
                    .ftpshipmentHeaderContinuationResponse)
                {
                    AddItemToCustomsResponse(shipmentHeaderContinuationResponse, item);
                    if (!string.IsNullOrEmpty(shipmentHeaderContinuationResponse.internalTransactionNumber))
                    {
                        ApplyCustomsSuccessStatus(item, shipmentHeaderContinuationResponse);
                    }
                    if (shipmentHeaderContinuationResponse.narrativeText.ToUpper().Contains("REJECTED"))
                    {
                        isShipmentRejected = true;
                        responseText = shipmentHeaderContinuationResponse.narrativeText;
                    }


                }
            }
            if (ftpcommodityShipment.ftptransportationGroup != null)
            {
                foreach (var transportationGroup in ftpcommodityShipment.ftptransportationGroup)
                {
                    if (transportationGroup.transportationDetailResponse != null)
                    {
                        foreach (var line in transportationGroup.transportationDetailResponse)
                        {
                            AddItemToCustomsResponse(line, item);
                            if (!string.IsNullOrEmpty(line.internalTransactionNumber))
                            {

                                ApplyCustomsSuccessStatus(item, line);
                            }
                            if (line.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                isShipmentRejected = true;
                                responseText = line.narrativeText;
                            }

                        }
                    }

                }
            }

            if (ftpcommodityShipment.ftpshipmentPartyGroup != null)
            {
                foreach (var shipmentPartyGroup in ftpcommodityShipment.ftpshipmentPartyGroup)
                {
                    if (shipmentPartyGroup.partyHeaderResponse != null)
                    {

                        foreach (var partyHeaderResponse in shipmentPartyGroup.partyHeaderResponse)
                        {
                            AddItemToCustomsResponse(partyHeaderResponse, item);
                            if (!string.IsNullOrEmpty(partyHeaderResponse.internalTransactionNumber))
                            {
                                ApplyCustomsSuccessStatus(item, partyHeaderResponse);
                            }
                            if (partyHeaderResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                isShipmentRejected = true;
                                responseText = partyHeaderResponse.narrativeText;
                            }
                        }
                    }

                    if (shipmentPartyGroup.partyAddressResponse != null)
                    {

                        foreach (var partyAddressResponse in shipmentPartyGroup.partyAddressResponse)
                        {
                            AddItemToCustomsResponse(partyAddressResponse, item);
                            if (!string.IsNullOrEmpty(partyAddressResponse.internalTransactionNumber))
                                ApplyCustomsSuccessStatus(item, partyAddressResponse);
                            if (partyAddressResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                isShipmentRejected = true;
                                responseText = partyAddressResponse.narrativeText;
                            }
                        }
                    }

                    if (shipmentPartyGroup.partyAddressContinuationResponse != null)
                    {
                        foreach (var partyAddressContinuationResponse in shipmentPartyGroup
                            .partyAddressContinuationResponse)
                        {
                            AddItemToCustomsResponse(partyAddressContinuationResponse, item);
                            if (!string.IsNullOrEmpty(partyAddressContinuationResponse.internalTransactionNumber))
                                ApplyCustomsSuccessStatus(item, partyAddressContinuationResponse);
                            if (partyAddressContinuationResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                isShipmentRejected = true;
                                responseText = partyAddressContinuationResponse.narrativeText;
                            }
                        }
                    }
                }
            }

            if (isShipmentCancelled)
            {
                item.SubmissionResponse.Status = "SUCCESS";
                item.SubmissionStatus = AesStatus.CANCELLED;
                item.SubmissionStatusDescription = responseText;
            }
           else if (isShipmentRejected)
            {
                item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                item.SubmissionStatusDescription = responseText;
            }
            else if (item.SubmissionStatus != AesStatus.CUSTOMSAPPROVED &&
                 item.SubmissionStatus != AesStatus.CUSTOMSREJECTED)
            {
                // nohting found on xml to decide success so failed

                item.SubmissionResponse.Status = "FAIL";
                item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                item.SubmissionStatusDescription = "Automatically set by AES, found no relevant information to process. Please see response details.";
                item.ShipmentHeader.OriginalItn = string.Empty;
            }
        }

        private static void ApplyCustomsSuccessStatus(Model.Aes item, FtpReponseStructure response)
        {
            item.SubmissionResponse.Status = "SUCCESS";
            item.SubmissionStatus = AesStatus.CUSTOMSAPPROVED;
            item.SubmissionStatusDescription = response.narrativeText;
            item.ShipmentHeader.OriginalItn = response.internalTransactionNumber;
        }  

        private void AddItemToCustomsResponse(FtpReponseStructure response, Model.Aes item)
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
        }
    }
}
