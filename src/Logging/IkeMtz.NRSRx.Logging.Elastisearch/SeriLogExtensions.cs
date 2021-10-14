using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace IkeMtz.NRSRx.Core.Web
{
  public static class SeriLogExtensions
  {
    public static IHostBuilder UseLogging(this IHostBuilder hostBuilder)
    {
      return hostBuilder.UseSerilog();
    }

    /// <summary>
    /// Sets up Console Logging only, leverages SeriLog sinks
    /// </summary>
    /// <param name="startup"></param>
    public static ILogger SetupConsoleLogging(this CoreWebStartup startup)
    {
      return Log.Logger = new LoggerConfiguration()
          .MinimumLevel.Debug()
          .Enrich.FromLogContext()
          .WriteTo.Console()
          .CreateLogger();
    }

    public static IApplicationBuilder UseSerilog(this IApplicationBuilder app)
    {
      return app.UseSerilogRequestLogging(options =>
      {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
          diagnosticContext.Set("UserName", httpContext.User?.Identity?.Name);
          diagnosticContext.Set("RemoteIpAddress", httpContext.Connection?.RemoteIpAddress?.ToString());
        };
      });
    }
  }
}
