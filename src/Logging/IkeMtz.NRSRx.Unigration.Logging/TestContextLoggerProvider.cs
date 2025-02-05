using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Unigration.Logging
{
  /// <summary>
  /// Provides a logger provider that creates <see cref="TestContextLogger"/> instances.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="TestContextLoggerProvider"/> class.
  /// </remarks>
  /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
  public sealed class TestContextLoggerProvider(TestContext testContext) : ILoggerProvider
  {
    private readonly TestContext testContext = testContext;
    private readonly ConcurrentDictionary<string, TestContextLogger> _loggers = new();

    /// <summary>
    /// Creates a new <see cref="TestContextLogger"/> instance.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>A <see cref="TestContextLogger"/> instance.</returns>
    public ILogger CreateLogger(string categoryName)
    {
      return _loggers.GetOrAdd(categoryName, name => new TestContextLogger(name, testContext));
    }

    /// <summary>
    /// Disposes the logger provider and clears the loggers.
    /// </summary>
    public void Dispose()
    {
      _loggers.Clear();
    }
  }
}
