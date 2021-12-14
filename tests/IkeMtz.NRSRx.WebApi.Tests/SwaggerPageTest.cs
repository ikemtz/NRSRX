using System.Threading.Tasks;
using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.NRSRx.Core.Unigration.Swagger;
using IkeMtz.Samples.WebApi;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.WebApi.Tests
{
  [TestClass]
  public class SwaggerPageTest : BaseUnigrationTests
  {
    [TestMethod]
    [TestCategory("Unigration")]
    public async Task GetSwaggerPageTest()
    {
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>());
      var htmlPage = await SwaggerUnitTests.TestHtmlPageAsync(srv);
      Assert.IsNotNull(htmlPage);
      var jsonDoc = await SwaggerUnitTests.TestJsonDocAsync(srv);
      _ = await SwaggerUnitTests.TestReverseProxyJsonDocAsync(srv);
      Assert.IsNotNull(jsonDoc);
    }
  }
}
