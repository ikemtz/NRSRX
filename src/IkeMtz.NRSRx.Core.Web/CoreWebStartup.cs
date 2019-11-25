using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IkeMtz.NRSRx.Core.Web
{
  public abstract class CoreWebStartup
  {
    public abstract string MicroServiceTitle { get; }
    public abstract Assembly StartupAssembly { get; }
    public virtual string JwtNameClaimMapping { get; } = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
    public virtual Dictionary<string, string> SwaggerScopes =>
        new Dictionary<string, string>{
                        { "openid", "required" }
                };
    public IConfiguration Configuration { get; }
    protected CoreWebStartup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public virtual void SetupLogging(IServiceCollection services)
    {
      services
          .AddApplicationInsightsTelemetry(Configuration.GetValue<string>("InstrumentationKey"));
    }

    public virtual AuthenticationBuilder SetupJwtAuthSchema(IServiceCollection services)
    {
      return services
          .AddAuthentication(options =>
          {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
          });
    }

    public virtual void SetupAuthentication(AuthenticationBuilder builder)
    {
      builder
          .AddJwtBearer(options =>
          {
            options.Authority = Configuration.GetValue<string>("IdentityProvider");
            options.TokenValidationParameters = new TokenValidationParameters()
            {
              NameClaimType = JwtNameClaimMapping,
              ValidAudiences = GetIdentityAudiences(),
            };
          });
    }

    private string[] GetIdentityAudiences()
    {
      return Configuration.GetValue<string>("IdentityAudiences")?.Split(',');
    }

    public void SetupSwagger(IServiceCollection services)
    {
      services.AddTransient<IConfigureOptions<SwaggerGenOptions>>(serviceProvider => new ConfigureSwaggerOptions(serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>(), this));
      services.AddSwaggerGen(options =>
      {
        // add a custom operation filter which sets default values
        options.OperationFilter<SwaggerDefaultValues>();
        var audiences = GetIdentityAudiences();
        if (audiences != null && audiences.Length != 0)
        {
          var audience = audiences.FirstOrDefault();

          options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
          {
            Type = SecuritySchemeType.OAuth2,
            In = ParameterLocation.Header,
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Flows = new OpenApiOAuthFlows
            {
              Implicit = new OpenApiOAuthFlow
              {
                AuthorizationUrl = new Uri($"{Configuration.GetValue<string>("SwaggerIdentityProviderUrl")}authorize?audience={audience}"),
                Scopes = SwaggerScopes,
              },
            }
          });
          options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme}
                        },
                       Array.Empty<string>()
                    }
                });
        }
      });
    }

    public virtual void SetupDatabase(IServiceCollection services, string connectionString) { }


    public virtual void SetupMvcOptions(IServiceCollection services, MvcOptions options)
    {
    }

    public virtual void SetupMiscDependencies(IServiceCollection services) { }
  }
}
