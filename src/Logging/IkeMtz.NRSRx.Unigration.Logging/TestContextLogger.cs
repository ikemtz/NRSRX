using IkeMtz.NRSRx.Core;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace IkeMtz.NRSRx.Unigration.Logging
{
  /// <summary>
  /// A logger implementation that writes log messages to the MSTest <see cref="TestContext"/>.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="TestContextLogger"/> class.
  /// </remarks>
  /// <param name="categoryName">The category name for the logger.</param>
  /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
  public class TestContextLogger(string categoryName, TestContext testContext) : ILogger
  {
    private readonly TestContext testContext = testContext;
    private readonly string categoryName = categoryName;

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>An <see cref="IDisposable"/> that ends the logical operation scope on dispose.</returns>
    public IDisposable BeginScope<TState>(TState state)
    {
      try
      {
        testContext.WriteLine($"**{categoryName}** Beginning State: {state?.ToString()}");
      }
      catch (Exception)
      {
        testContext.WriteLine($"**{categoryName}** Beginning State: Too Large To specify");
      }
      return new TestContextOperationScope<TState>(this.testContext, state);
    }

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>Always returns true as all log levels are enabled.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
      return true;
    }

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event ID.</param>
    /// <param name="state">The state object.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="formatter">The function to create a log message.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
      if (formatter != null)
      {
        testContext.WriteLine(formatter(state, exception));
      }
      else
      {
        testContext.WriteLine($"**{categoryName}** {logLevel}:");
        testContext.WriteLine($"State: {state?.ToString()}");
        if (exception != null)
        {
          testContext.WriteLine($"Exception: {JsonConvert.SerializeObject(exception, Constants.JsonSerializerSettings)}");
        }
      }
    }
  }
}
