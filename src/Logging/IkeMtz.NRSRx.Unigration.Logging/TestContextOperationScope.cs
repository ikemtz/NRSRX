using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Unigration.Logging
{
  /// <summary>
  /// Represents a scope for logging operations within a test context.
  /// </summary>
  /// <typeparam name="TState">The type of the state object associated with the scope.</typeparam>
  public sealed class TestContextOperationScope<TState> : IDisposable
  {
    private readonly TestContext testContext;
    private readonly Stopwatch stopWatch;
    private readonly TState state;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestContextOperationScope{TState}"/> class.
    /// </summary>
    /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
    /// <param name="state">The state object associated with the scope.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="testContext"/> is null.</exception>
    public TestContextOperationScope(TestContext testContext, TState state)
    {
      testContext = testContext ?? throw new ArgumentNullException(nameof(testContext));
      this.testContext = testContext;
      this.state = state;
      this.stopWatch = new Stopwatch();
      this.stopWatch.Start();
      this.testContext.WriteLine($"Starting Scope: {state} @ {DateTime.Now.ToShortDateString()}");
    }

    /// <summary>
    /// Disposes the scope and logs the total elapsed time.
    /// </summary>
    public void Dispose()
    {
      this.stopWatch.Stop();
      this.testContext.WriteLine($"Ending Scope: {state} Total Milliseconds: {this.stopWatch.ElapsedMilliseconds}");
    }
  }
}
