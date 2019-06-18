using AspNetCore.Mvc.MvcAsApi;
using AspNetCore.Mvc.MvcAsApi.Conventions;
using AspNetCore.Mvc.MvcAsApi.Factories;
using AspNetCore.Mvc.MvcAsApi.Middleware;
using AspNetCore.Mvc.MvcAsApi.ModelBinding;
using DND.Common.Domain.ModelMetadata;
using DynamicForms.DisplayConventions;
using DynamicForms.Razor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DynamicForms.Web
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvcDisplayConventions(
                new HtmlByNameConventionFilter(), 
                new LabelTextConventionFilter(), 
                new TextAreaByNameConventionFilter(), 
                new TextboxPlaceholderConventionFilter(),
                new DisableConvertEmptyStringToNull());

            services.AddMvcDisplayAttributes();
            services.AddInheritanceValidationAttributeAdapterProvider();
            services.AddDynamicForms();

             
            services.AddTransient<IStartupFilter, StartupFilter1>();
            services.AddTransient<IStartupFilter, StartupFilter2>();

            services.AddMvc(options =>
            {
                options.Conventions.Add(new MvcAsApiConvention());

                //Return problem details in json/xml if an error response is returned via Api
                //options.Conventions.Add(new ApiErrorFilterConvention());
                //Return problem details in json/xml if an exception is thrown via Api
                //options.Conventions.Add(new ApiExceptionFilterConvention());
                //Post data to MVC Controller from API
                //options.Conventions.Add(new FromBodyAndOtherSourcesConvention(true, true, true));
                //Return data uisng output formatter when acccept header is application/json or application/xml
                //options.Conventions.Add(new ConvertViewResultToObjectResultConvention());

                var modelMinderProviders = options.ModelBinderProviders;
                var output = options.OutputFormatters;
                var input = options.InputFormatters;

            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddDynamicModelBinder();
            // .AddXmlSerializerFormatters()

            services.AddEnhancedProblemDetailsClientErrorFactory(true);
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.EnableEnhancedValidationProblemDetails();
            });

            services.AddViewRenderer();
        }

        public class StartupFilter1 : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return builder =>
                {
                    next(builder);
                };
            }
        }

        public class StartupFilter2 : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return builder =>
                {
                    next(builder);
                };
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // Non Api
                app.UseWhen(context => context.Request.IsMvc(),
                    appBranch =>
                    {
                        appBranch.UseDeveloperExceptionPage();
                    }
               );

                // Web Api
                app.UseWhen(context => context.Request.IsApi(),
                    appBranch =>
                    {
                        appBranch.UseWebApiExceptionHandlerProblemDetails(true);
                        appBranch.UseWebApiErrorHandlerProblemDetails();
                    }
               );

                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
