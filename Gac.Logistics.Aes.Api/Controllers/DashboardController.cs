using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Model;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AesDbRepository aesDbRepository;
        public DashboardController(AesDbRepository aesDbRepository)
        {
            this.aesDbRepository = aesDbRepository;
        }

        [HttpGet("getsummary")]
        public ActionResult GetSummary()
        {
            var list = new List<DashboardSummary>()
            {
                new DashboardSummary()
                {
                    Name = "Pending",
                    Icon = "loading",
                    Count = this.aesDbRepository.FindItemsCount<Api.Model.Aes>(x => x.SubmissionStatus == AesStatus.PENDING ||  x.SubmissionStatus == AesStatus.DRAFT)
                },
                new DashboardSummary()
                {
                    Name = "Rejected",
                    Icon = "cancel",
                    Count = this.aesDbRepository.FindItemsCount<Api.Model.Aes>(x => x.SubmissionStatus == AesStatus.GETSREJECTED ||  x.SubmissionStatus == AesStatus.CUSTOMSREJECTED)
                },
                new DashboardSummary()
                {
                    Name = "Approved",
                    Icon = "checked",
                    Count = this.aesDbRepository.FindItemsCount<Api.Model.Aes>(x => x.SubmissionStatus == AesStatus.GETSAPPROVED ||  x.SubmissionStatus == AesStatus.CUSTOMSAPPROVED)
                },
            };

            return new ObjectResult(list);
        }


        [HttpGet("getallbystatus")]
        public ActionResult GetAllByStatus(string status)
        {
            Expression<Func<Api.Model.Aes, bool>> predicate = x=>x.Id != null;
            if (status == "Pending")
            {
                predicate = x => x.SubmissionStatus == AesStatus.PENDING || x.SubmissionStatus == AesStatus.DRAFT;
            }
            else if (status == "Approved")
            {
                predicate = x => x.SubmissionStatus == AesStatus.GETSAPPROVED || x.SubmissionStatus == AesStatus.CUSTOMSAPPROVED;
            }
            else if (status == "Rejected")
            {
                predicate = x => x.SubmissionStatus == AesStatus.GETSREJECTED || x.SubmissionStatus == AesStatus.CUSTOMSREJECTED;
            }

            var query = this.aesDbRepository.GetItemsAsync(predicate).Result;
            var items = query.Select(x => new
            {
                x.Id,
                x.ShipmentHeader?.ShipmentReferenceNumber,
                x.ShipmentHeader?.EstimatedExportDate,
                x.ShipmentHeader?.OriginalItn,
                x.SubmissionStatus,
                x.SubmissionStatusDescription,
            });
            return Ok(items);
        }
    }
}
