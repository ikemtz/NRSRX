using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IkeMtz.NRSRx.Jobs.Core
{
  /// <summary>
  /// Represents a job within the NRSRx framework.
  /// </summary>
  public interface IJob
  {
    /// <summary>
    /// Gets or sets the application configuration.
    /// </summary>
    IConfiguration Configuration { get; set; }

    /// <summary>
    /// Gets or sets the job host.
    /// </summary>
    IHost JobHost { get; set; }

    /// <summary>
    /// Sets up the dependencies required by the job.
    /// </summary>
    /// <param name="services">The service collection to add the dependencies to.</param>
    /// <returns>The service collection with the dependencies added.</returns>
    IServiceCollection SetupDependencies(IServiceCollection services);

    /// <summary>
    /// Sets up the functions to be executed by the job.
    /// </summary>
    /// <param name="services">The service collection to add the functions to.</param>
    /// <returns>The service collection with the functions added.</returns>
    IServiceCollection SetupFunctions(IServiceCollection services);
  }
}
