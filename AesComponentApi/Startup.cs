using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Swashbuckle.AspNetCore.Swagger;

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
        
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddCors(options =>
                             {
                                 options.AddPolicy("CorsPolicy",
                                                   builder => builder.AllowAnyOrigin()
                                                                     .AllowAnyMethod()
                                                                     .AllowAnyHeader()
                                                                     .AllowCredentials());
                             });


            services.AddAutoMapper();
            services.AddSwaggerGen(c =>
                                   {
                                       c.SwaggerDoc("v1", new Info
                                                          {
                                                              Version = "v1",
                                                              Title = "AES COMPONENT API",
                                                              Description = "Web api for the aes component",                                                             
                                                          });
                                   });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseCors("CorsPolicy");
           
        }
    }
}
