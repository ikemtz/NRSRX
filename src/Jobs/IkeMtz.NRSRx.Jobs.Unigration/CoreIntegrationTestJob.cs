using IkeMtz.NRSRx.Jobs.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Jobs.Unigration
{
  /// <summary>
  /// Represents a core integration test job for testing purposes.
  /// </summary>
  /// <typeparam name="TProgram">The type of the program implementing the <see cref="IJob"/> interface.</typeparam>
  /// <remarks>
  /// Initializes a new instance of the <see cref="CoreIntegrationTestJob{TProgram}"/> class.
  /// </remarks>
  /// <param name="program">The program instance.</param>
  /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
  public class CoreIntegrationTestJob<TProgram>(TProgram program, TestContext testContext) : CoreUnigrationTestJob<TProgram>(program, testContext)
          where TProgram : class, IJob
  {
  }
}
