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
                AckGetsReponse getsRes;
                getsRes = getsResponse;
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

            //var ftpcommodityShipment = customsReponse.ftpaesResponse.ftpcommodityShipment;

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
                var item = aesDbRepository.GetItemsAsync<Model.Aes>(obj => obj.ShipmentHeader.ShipmentReferenceNumber == ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber)
                                          .Result
                                          .FirstOrDefault();
                if (item == null)
                {
                    return BadRequest($"Invalid shipment reference no ${ftpcommodityShipment.ftpshipmentHeader.shipmentReferenceNumber}");
                }

                // important function go and see inside
                ProcessCustomsStructure(ftpcommodityShipment, item);

                await aesDbRepository.UpdateItemAsync(item.Id, item);

                // signalr notoificatio
                await hubContext.Clients.All.SendAsync("customscallback", new
                {
                    itn = item.ShipmentHeader.OriginalItn,
                    status = item.SubmissionResponse.Status,
                    description = item.SubmissionStatus,
                    errorList = item.SubmissionResponse.CustomsResponseList
                });


            }

            return Ok(true);
        }

        private void ProcessCustomsStructure(Ftpcommodityshipment ftpcommodityShipment, Model.Aes item)
        {
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
                    else if (shipmentHeaderResponse.narrativeText.ToUpper().Contains("REJECTED"))
                    {
                        item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                        item.SubmissionStatusDescription = shipmentHeaderResponse.narrativeText;
                    }
                }
            }
            if (ftpcommodityShipment.ftpcommodityLineItemGroup != null)
            {
                foreach (var commodityLineItemGroup in ftpcommodityShipment.ftpcommodityLineItemGroup)
                {

                    foreach (var lineItemHeaderResponse in commodityLineItemGroup.ftplineItemHeaderResponse)
                    {
                        AddItemToCustomsResponse(lineItemHeaderResponse, item);

                        if (!string.IsNullOrEmpty(lineItemHeaderResponse.internalTransactionNumber))
                        {
                            ApplyCustomsSuccessStatus(item, lineItemHeaderResponse);
                        }
                        else if (lineItemHeaderResponse.narrativeText.ToUpper().Contains("REJECTED"))
                        {
                            item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                            item.SubmissionStatusDescription = lineItemHeaderResponse.narrativeText;
                        }
                    }

                    foreach (var lineItemHeaderContinuationResponse in commodityLineItemGroup.ftplineItemHeaderContinuationResponse)
                    {
                        AddItemToCustomsResponse(lineItemHeaderContinuationResponse, item);
                        if (!string.IsNullOrEmpty(lineItemHeaderContinuationResponse.internalTransactionNumber))
                        {
                            ApplyCustomsSuccessStatus(item, lineItemHeaderContinuationResponse);

                        }
                        else if (lineItemHeaderContinuationResponse.narrativeText.ToUpper().Contains("REJECTED"))
                        {
                            item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                            item.SubmissionStatusDescription = lineItemHeaderContinuationResponse.narrativeText;
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
                            else if (licenseDetailResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                                item.SubmissionStatusDescription = licenseDetailResponse.narrativeText;
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
                    else if (shipmentHeaderContinuationResponse.narrativeText.ToUpper().Contains("REJECTED"))
                    {
                        item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                        item.SubmissionStatusDescription = shipmentHeaderContinuationResponse.narrativeText;
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
                            if (string.IsNullOrEmpty(line.internalTransactionNumber))
                            {

                                ApplyCustomsSuccessStatus(item, line);
                            }
                            else if (line.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                ApplyCustomsFailureStatus(item, line);
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
                            else if (partyHeaderResponse.narrativeText.ToUpper().Contains("REJECTED"))
                            {
                                ApplyCustomsFailureStatus(item, partyHeaderResponse);
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
                            else if (partyAddressResponse.narrativeText.ToUpper().Contains("REJECTED"))
                                ApplyCustomsFailureStatus(item, partyAddressResponse);
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
                            else if (partyAddressContinuationResponse.narrativeText.ToUpper().Contains("REJECTED"))
                                ApplyCustomsFailureStatus(item, partyAddressContinuationResponse);
                        }
                    }
                }
            }

            if (item.SubmissionStatus != AesStatus.CUSTOMSAPPROVED ||
                item.SubmissionStatus != AesStatus.CUSTOMSREJECTED)
            {
                // nohting found on xml to decide success so failed

                item.SubmissionResponse.Status = "FAIL";
                item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
                item.SubmissionStatusDescription = "Automatically set by AES , found no relvant information to process in customs response";
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

        private static void ApplyCustomsFailureStatus(Model.Aes item, FtpReponseStructure response)
        {
            item.SubmissionStatus = AesStatus.CUSTOMSREJECTED;
            item.SubmissionStatusDescription = response.narrativeText;
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
