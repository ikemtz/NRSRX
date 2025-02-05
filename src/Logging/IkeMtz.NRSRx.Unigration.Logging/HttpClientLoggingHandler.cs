using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Unigration.Logging
{
  /// <summary>
  /// A delegating handler that logs HTTP client requests and responses for testing purposes.
  /// </summary>
  /// <remarks>
  /// Initializes a new instance of the <see cref="HttpClientLoggingHandler"/> class.
  /// </remarks>
  /// <param name="testContext">The test context for logging.</param>
  /// <param name="testingHttpMessageHandler">The inner HTTP message handler.</param>
  public class HttpClientLoggingHandler(TestContext testContext, HttpMessageHandler testingHttpMessageHandler) : DelegatingHandler(testingHttpMessageHandler)
  {
    /// <summary>
    /// Gets the test context for logging.
    /// </summary>
    public TestContext TestContext { get; } = testContext;

    /// <summary>
    /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      TestContext.WriteLine("Http Client Message Request:");
      TestContext.WriteLine($"  Request Uri: {request.RequestUri}");
      TestContext.WriteLine($"  Request Method: {request.Method}");
      TestContext.WriteLine($"  Request Version: {request.Version}");
      request.Headers
        .Where(t => t.Value.Any(a => !string.IsNullOrWhiteSpace(a)))
        .ToList()
        .ForEach(x =>
        {
          TestContext.WriteLine($"  Request Header: {x.Key} = {string.Join(";", x.Value)}");
        });
      if (request.Content != null)
      {
        TestContext.WriteLine($"  Request Content: {await request.Content.ReadAsStringAsync(cancellationToken)}");
      }
      else
      {
        TestContext.WriteLine($"  Request Content: << NO CONTENT >>");
      }

      var response = await base.SendAsync(request, cancellationToken);

      TestContext.WriteLine("Http Client Message Response:");
      TestContext.WriteLine($"  Response Status Code: {(int)response.StatusCode} {response.StatusCode}");
      TestContext.WriteLine($"  Response Status Version: {response.Version}");
      response.Headers
        .Where(t => t.Value.Any(a => !string.IsNullOrWhiteSpace(a)))
        .ToList()
        .ForEach(x =>
        {
          TestContext.WriteLine($"  Response Header: {x.Key} = {string.Join(";", x.Value)}");
        });

      if (response.Content != null)
      {
        TestContext.WriteLine($"  Response Content: {await response.Content.ReadAsStringAsync(cancellationToken)}");
      }
      else
      {
        TestContext.WriteLine($"  Response Content: << NO CONTENT >>");
      }
      return response;
    }
  }
}
