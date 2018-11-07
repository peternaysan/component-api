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

        private readonly IMapper mapper;

        public LookUpController(CountryDbRepository countryDbRepository, IMapper mapper, HtsDbRepository htsDbRepository)
        {
            this.countryDbRepository = countryDbRepository;
            this.htsDbRepository = htsDbRepository;
            this.mapper = mapper;

        }

        [HttpGet]
        public async Task<ActionResult> GetAllStates(string country)
        {          
            var items =  await this.countryDbRepository.GetItemsAsync<Country>(obj=>obj.Name.ToLower().Contains(country.ToLower()));
            var states = items.Select(obj => obj.States).First();
            return new ObjectResult(states);
        }

        [HttpGet("gethtscode")]
        public async Task<ActionResult> GetHtsCode(string term)
        {
            var items = await this.htsDbRepository.GetItemsAsync<HtsCode>(obj => obj.Name.ToLower().Contains(term.ToLower()) ||obj.Code.ToLower().Contains(term.ToLower()));            
            return new ObjectResult(items);
        }

        [HttpGet("getlicexemptioncode")]
        public async Task<ActionResult> GetLicenseExemptionCode(string term)
        {
            var items = await this.htsDbRepository.GetItemsAsync<LicenseExemptionCode>(obj => obj.Name.ToLower().Contains(term.ToLower()) || obj.Code.ToLower().Contains(term.ToLower()));
            return new ObjectResult(items);
        }

    }
}
