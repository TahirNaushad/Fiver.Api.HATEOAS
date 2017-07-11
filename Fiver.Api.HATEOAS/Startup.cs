using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Fiver.Api.HATEOAS.OtherLayers;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace Fiver.Api.HATEOAS
{
    public class Startup
    {
        public void ConfigureServices(
            IServiceCollection services)
        {
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(factory =>
            {
                var actionContext = factory.GetService<IActionContextAccessor>()
                                           .ActionContext;
                return new UrlHelper(actionContext);
            });

            services.AddSingleton<IMovieService, MovieService>();

            services.AddMvc(options =>
            {
                options.ReturnHttpNotAcceptable = true;

                options.InputFormatters
                       .OfType<JsonInputFormatter>()
                       .FirstOrDefault()
                       ?.SupportedMediaTypes.Add("application/vnd.fiver.movie.input+json");

                options.OutputFormatters
                       .OfType<JsonOutputFormatter>()
                       .FirstOrDefault()
                       ?.SupportedMediaTypes.Add("application/vnd.fiver.hateoas+json");
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver =
                    new CamelCasePropertyNamesContractResolver();
            });
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler(configure =>
            {
                configure.Run(async context =>
                {
                    var ex = context.Features
                                    .Get<IExceptionHandlerFeature>()
                                    .Error;

                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync($"{ex.Message}");
                });
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
