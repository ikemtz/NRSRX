using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace IkeMtz.NRSRx.Core.Web
{
  /// <summary>
  /// Extension methods to setup logging on NRSRx framework
  /// </summary>
  public static class StartupExtensions
  {
    /// <summary>
    /// Sets up the asp.net core application to log to a Elastisearch instance.
    /// Refernce: https://github.com/serilog/serilog-sinks-elasticsearch/wiki/basic-setup
    /// Note: The following configuration variables are required:
    /// ELASTISEARCH_HOST => The url of the Elastisearch endpoint (ie: http(s)://{Elastisearch Host}/9200)
    /// </summary>
    /// <param name="startup"></param>
    /// <param name="app"></param>
    public static ILogger SetupElastisearch(this CoreWebStartup startup, IApplicationBuilder app)
    {
      if (SeriLogExtensions.Logger != null)
      {
        return SeriLogExtensions.Logger;
      }
      app?.UseSerilog();
      var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

      var host = startup.Configuration.GetValue<string>("ELASTISEARCH_HOST", "http://localhost:9200");
      var username = startup.Configuration.GetValue<string>("ELASTISEARCH_USERNAME");
      var password = startup.Configuration.GetValue<string>("ELASTISEARCH_PASSWORD");
      var apiKey = startup.Configuration.GetValue<string>("ELASTISEARCH_API_KEY");
      var elastiOptions = new ElasticsearchSinkOptions(new Uri(host))
      {
        IndexFormat = $"{startup.StartupAssembly.GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
      };
      if (!string.IsNullOrWhiteSpace(password))
      {
        elastiOptions.ModifyConnectionSettings = x => x.BasicAuthentication(username, password);
      }
      else if (!string.IsNullOrWhiteSpace(apiKey))
      {
        elastiOptions.ModifyConnectionSettings = x => x.ApiKeyAuthentication(username, apiKey);
      }
      return SeriLogExtensions.Logger = Log.Logger = new LoggerConfiguration() { }
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.Elasticsearch()
        .Enrich.WithProperty("Environment", environment)
        .CreateLogger();
    }
  }
}
