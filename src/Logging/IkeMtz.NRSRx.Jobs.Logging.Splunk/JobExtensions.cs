using IkeMtz.NRSRx.Logging.Splunk;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace IkeMtz.NRSRx.Jobs.Core;

/// <summary>
/// Extension methods to setup logging on NRSRx framework
/// </summary>
public static class JobExtensions
{
  /// <summary>
  /// Sets up the asp.net core application to log to a Splunk instance using a Splunk Http Event Collector.
  /// Refernce: https://docs.splunk.com/Documentation/SplunkCloud/8.2.2106/Data/UsetheHTTPEventCollector
  /// Note: The following configuration variables are required:
  /// SPLUNK_HOST => The url of the splunk collector endpoint (ie: http(s)://{Splunk Host}/services/collector)
  /// SPLUNK_TOKEN => Security token 
  /// </summary>
  /// <param name="job"></param>
  /// <param name="services"></param>
  public static ILogger SetupSplunk(this IJob job, IServiceCollection services)
  {
    _ = SplunkExtensions.ConfigureSplunkLogger(job.Configuration);
    _ = services.AddLogging(x => x.AddSerilog());
    return Log.Logger;
  }
}

