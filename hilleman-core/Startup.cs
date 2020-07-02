using System;
using com.bitscopic.hilleman.core.domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace com.bitscopic.hilleman.core
{
    public class Startup
    {
        public Startup(IHostingEnvironment environment)
        {
            IConfigurationBuilder config = new ConfigurationBuilder()
                .SetBasePath(environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            this.Configuration = config.Build();
            MyConfigurationManager.getValue("BuildConfiguration");
        }

        public Startup(IConfiguration configuration)
        {
            IConfigurationBuilder config = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            this.Configuration = config.Build();
            MyConfigurationManager.getValue("BuildConfiguration");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSwaggerGen(swagger => swagger.SwaggerDoc("v0.1", new Swashbuckle.AspNetCore.Swagger.Info()
            {
                Contact = new Swashbuckle.AspNetCore.Swagger.Contact() { Email = "" },
                Description = "Bitscopic Web Services",
                Title = "API Documentation",
                Version = "0.1"
            }));
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
                //app.UseHsts();
            }

            app.UseCors(builder => builder
                                    .AllowAnyHeader()
                                    .AllowAnyMethod()
                                    .AllowAnyOrigin()
                                    
                                  //  .WithHeaders(new string[] { "x-requested-with", "Content-Type", "Accept" })
                                  //  .WithExposedHeaders(new string[] { "Access-Control-Session-Token", "Access-Control-App-Token" })
                                  //  .WithMethods(new string[] { "GET", "POST", "PUT", "DELETE" })
                                  //  .AllowAnyOrigin()
                                    .SetPreflightMaxAge(TimeSpan.FromSeconds(3600)));

            //app.UseHillemanSessionMiddleware();
            //app.UseHttpsRedirection();
            app.UseMvc();


            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v0.1/swagger.json", "API Documentation v0.1");
            });

        }
    }
}
