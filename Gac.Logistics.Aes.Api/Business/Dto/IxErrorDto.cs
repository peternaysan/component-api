using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Gac.Logistics.Aes.Api.Business.Dto
{
    public class IxErrorDto
    {
        public string ErrorMessage { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }
    }
}
