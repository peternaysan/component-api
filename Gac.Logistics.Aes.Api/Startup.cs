using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Gac.Logistics.Aes.Api.Data;
using Gac.Logistics.Aes.Api.Model;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Rewrite;
using Gac.Logistics.Aes.Api.Business;
using System;

namespace AesComponentApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get;  }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Repositories
            services.AddTransient<AesDbRepository, AesDbRepository>();
            services.AddTransient<AesTransactionDbRepository, AesTransactionDbRepository>();
            services.AddTransient<CountryDbRepository, CountryDbRepository>();

            services.AddTransient<IxService, IxService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddCors(options =>
                             {
                                 options.AddPolicy("CorsPolicy",
                                                   builder => builder.AllowAnyOrigin()
                                                                     .AllowAnyMethod()
                                                                     .AllowAnyHeader()
                                                                     .AllowCredentials());
                             });

            services.AddHttpClient("ix", c =>
            {
                c.BaseAddress = new Uri(this.Configuration["AppSettings:IxEndpoint"]);
                //c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
            });

            services.AddAutoMapper();
            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new Info
                                                          {
                                                              Version = "v1",
                                                              Title = "Gac Losgistics Aes Api",
                                                              Description = "Web api for the aes component",                                                             
                                                          });
                                   });
            // Make Configuration injectable
            services.AddSingleton(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            DocumentDbInitializer.Initialize(Configuration);
            app.UseCors("CorsPolicy");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
                             {
                                 c.SwaggerEndpoint("/swagger/v1/swagger.json", "AES API v1.0");
                             });

            var option = new RewriteOptions();
            option.AddRedirect("^$", "swagger");
            app.UseRewriter(option);
            app.UseMvc();
        }
    }
}
