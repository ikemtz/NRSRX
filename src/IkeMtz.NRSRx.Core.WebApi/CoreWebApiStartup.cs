using IkeMtz.NRSRx.Core.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using WebApiContrib.Core.Formatter.MessagePack;

namespace IkeMtz.NRSRx.Core.WebApi
{
  public abstract class CoreWebApiStartup : CoreWebStartup
  {
    protected CoreWebApiStartup(IConfiguration configuration) : base(configuration)
    {
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      SetupLogging(services);
      SetupSwagger(services);
      SetupDatabase(services, Configuration.GetValue<string>("SqlConnectionString"));
      SetupPublishers(services);
      SetupAuthentication(SetupJwtAuthSchema(services));
      SetupMiscDependencies(services);
      _ = SetupCoreEndpointFunctionality(services)
         .AddApplicationPart(StartupAssembly)
         .AddControllersAsServices();
      _ = services.AddControllers();
    }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
    {
      if (env.IsDevelopment())
      {
        _ = app.UseDeveloperExceptionPage();
      }
      else
      {
        _ = app.UseHsts();
      }
      _ = app
       .UseRouting()
       .UseAuthentication()
       .UseAuthorization()
       .UseSwagger()
       .UseSwaggerUI(options =>
       {
         // build a swagger endpoint for each discovered API version
         foreach (var description in provider.ApiVersionDescriptions)
         {
           options.SwaggerEndpoint($"./swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
         }
         options.EnableDeepLinking();
         options.EnableFilter();
         options.RoutePrefix = string.Empty;
         options.HeadContent += "<meta name=\"robots\" content=\"none\" />";
         options.OAuthClientId(Configuration.GetValue<string>("SwaggerClientId"));
         options.OAuthAppName(Configuration.GetValue<string>("SwaggerAppName"));
       })
       .UseEndpoints(endpoints =>
      {
        _ = endpoints.MapControllers();
      });
    }

    public IMvcBuilder SetupCoreEndpointFunctionality(IServiceCollection services)
    {
      var builder = services
           .AddMvc(options =>
           {
             options.EnableEndpointRouting = true;
             options.FormatterMappings.SetMediaTypeMappingForFormat("xml", MediaTypeHeaderValue.Parse("application/xml"));
             SetupMvcOptions(services, options);
           })
           .AddNewtonsoftJson(options =>
           {
             options
             .UseCamelCasing(true)
             .SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

             options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
           })
           .AddMessagePackFormatters()
           .AddXmlSerializerFormatters()
           .SetCompatibilityVersion(CompatibilityVersion.Latest);
      _ = services
           .AddApiVersioning(options =>
           {
             // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
             options.ReportApiVersions = true;
             options.ApiVersionReader = new UrlSegmentApiVersionReader();
           })
           .AddVersionedApiExplorer(options =>
           {
             // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
             // note: the specified format code will format the version as "'v'major[.minor][-status]"
             options.GroupNameFormat = "'v'VVV";

             // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
             // can also be used to control the format of the API version in route templates
             options.SubstituteApiVersionInUrl = true;
           });
      return builder;
    }

    public virtual void SetupPublishers(IServiceCollection services) { }
  }
}
