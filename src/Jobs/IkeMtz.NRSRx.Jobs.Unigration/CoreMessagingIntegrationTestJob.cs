using IkeMtz.NRSRx.Jobs.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Jobs.Unigration
{
  /// <summary>
  /// Represents a core messaging integration test job for testing purposes.
  /// </summary>
  /// <typeparam name="TProgram">The type of the program implementing the <see cref="IJob"/> interface.</typeparam>
  /// <remarks>
  /// Initializes a new instance of the <see cref="CoreMessagingIntegrationTestJob{TProgram}"/> class.
  /// </remarks>
  /// <param name="program">The program instance.</param>
  /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
  public class CoreMessagingIntegrationTestJob<TProgram>(TProgram program, TestContext testContext) : CoreMessagingUnigrationTestJob<TProgram>(program, testContext)
          where TProgram : class, IJob
  {

    /// <summary>
    /// Sets up the dependencies for the job.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection with dependencies set up.</returns>
    public override IServiceCollection SetupDependencies(IServiceCollection services)
    {
      Program.Configuration = this.Configuration;
      return base.SetupDependencies(services);
    }
  }
}
