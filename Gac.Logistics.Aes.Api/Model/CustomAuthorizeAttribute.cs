using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace Gac.Logistics.Aes.Api.Model
{
    public class CustomAuthorizeAttribute :Attribute, IAuthorizationFilter
    {
        private readonly IConfiguration configuration;

        public CustomAuthorizeAttribute()
        {
           
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var autorizationKey = context.HttpContext.Request.Headers.FirstOrDefault(x => x.Key == "Authorization");
            var key = "thtCYLEj80g8zLP3M9LqxhK0ySTdPEopxEZeIZUM4aoZsJQIKPz3g5rddPIjGNDhA1yem0BQU1TDgN7gLPWyKd==";//this.configuration["AppSettings:ApiSecretKey"];

                if (autorizationKey.Value != key)
                {
                    context.Result = new ForbidResult();
                }
        }
    }
}
