using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Model;
using Gac.Logistics.Aes.Api.Model.SubClasses;
using Microsoft.AspNetCore.Mvc;

namespace Gac.Logistics.Aes.Api.Controllers
{
    [Route("api/lookup")]
    [ApiController]
    public class LookUpController
    {
        private readonly CountryDbRepository countryDbRepository;
        private readonly HtsDbRepository htsDbRepository;
        private readonly LicenseExemptionCodeDbRepository licenseExemptionCodeDbRepository;
        private readonly ExportInformationCodeDbRepository exportInformationCodeDbRepository;


        private readonly IMapper mapper;

        public LookUpController(CountryDbRepository countryDbRepository,
                                IMapper mapper,
                                HtsDbRepository htsDbRepository,
                                LicenseExemptionCodeDbRepository licenseExemptionCodeDbRepository,
                                ExportInformationCodeDbRepository exportInformationCodeDbRepository)
        {
            this.countryDbRepository = countryDbRepository;
            this.htsDbRepository = htsDbRepository;
            this.mapper = mapper;
            this.licenseExemptionCodeDbRepository = licenseExemptionCodeDbRepository;
            this.exportInformationCodeDbRepository = exportInformationCodeDbRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetAllStates(string country)
        {
            var items = await this.countryDbRepository.GetItemsAsync<Country>(obj => obj.Name.ToLower().Contains(country.ToLower()));
            var states = items.Select(obj => obj.States).First();
            return new ObjectResult(states);
        }

        [HttpGet("gethtscode")]
        public async Task<ActionResult> GetHtsCode(string term)
        {
            var items = await this.htsDbRepository
                        .GetTopItemsAsync<HtsCode>(obj => obj.Name.ToLower().Contains(term.ToLower()) || obj.Code.ToLower().Contains(term.ToLower()), 10);
            return new ObjectResult(items);
        }

        [HttpGet("getlicexemptioncode")]
        public async Task<ActionResult> GetLicenseExemptionCode()
        {
            var items = await this.licenseExemptionCodeDbRepository.GetItemsAsync<LicenseExemptionCode>();
            var mappedItems = items.Select(x => new
                                                {
                                                    name = x.Name,
                                                    code = x.Code
                                                });
            return new ObjectResult(mappedItems);
        }

        [HttpGet("getexportinformationcode")]
        public async Task<ActionResult> GetExportInformationCode()
        {
            var items = await this.exportInformationCodeDbRepository.GetItemsAsync<ExportInformationCode>();
            var mappedItems = items.Select(x => new
                                       {
                                           name = x.Description,
                                           code = x.Code
                                       });
            
            return new ObjectResult(mappedItems);
        }

    }
}
