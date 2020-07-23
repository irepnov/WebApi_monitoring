using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Middleware.Logging;

namespace WebService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //adding health check services to container
            services.AddHealthChecks()
                .AddCheck(name: "self", () => HealthCheckResult.Healthy())
                .AddDiskStorageHealthCheck(
                    opt => {
                        opt.AddDrive(@"C:\", 100);
                    }, 
                    name: "disc", failureStatus: HealthStatus.Unhealthy);
            //adding health check UI services
            services.AddHealthChecksUI().AddInMemoryStorage();
            services.AddLogging(builder => builder.AddConsole());
            services.AddRequestLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();
            app.UseRequestLogging("hc-ui");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //adding health check point used by the UI
            app.UseHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            //adding health check UI
            app.UseHealthChecksUI(delegate (Options option)
            {
                option.UIPath = "/hc-ui";
                option.AddCustomStylesheet("custom.css");
            });

            
        }
    }
}
