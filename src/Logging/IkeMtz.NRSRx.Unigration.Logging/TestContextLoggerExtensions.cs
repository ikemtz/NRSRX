using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Unigration.Logging
{
  /// <summary>
  /// Provides extension methods for adding a <see cref="TestContextLogger"/> to the logging builder.
  /// </summary>
  public static class TestContextLoggerExtensions
  {
    /// <summary>
    /// Adds a <see cref="TestContextLogger"/> to the logging builder.
    /// </summary>
    /// <param name="builder">The logging builder.</param>
    /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
    /// <returns>The logging builder with the <see cref="TestContextLogger"/> added.</returns>
    public static ILoggingBuilder AddTestContext(this ILoggingBuilder builder, TestContext testContext)
    {
      builder
          .SetMinimumLevel(LogLevel.Trace)
          .AddDebug()
          .Services
          .Add(ServiceDescriptor.Singleton<ILoggerProvider, TestContextLoggerProvider>((x) => new TestContextLoggerProvider(testContext)));
      return builder;
    }
  }
}
