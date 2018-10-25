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
        private readonly IMapper mapper;

        public LookUpController(CountryDbRepository countryDbRepository, IMapper mapper)
        {
            this.countryDbRepository = countryDbRepository;
            this.mapper = mapper;

        }

        [HttpGet]
        public async Task<ActionResult> GetAllStates(string country)
        {          
            var items =  await this.countryDbRepository.GetItemsAsync<Country>(obj=>obj.Name.ToLower().Contains(country.ToLower()));
            var states = items.Select(obj => obj.States).First();
            return new ObjectResult(states);
        }

    }
}
