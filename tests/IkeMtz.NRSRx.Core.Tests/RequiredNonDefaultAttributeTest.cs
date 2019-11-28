using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.NRSRx.Core.Unigration.WebApi.Unit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;

namespace IkeMtz.NRSRx.Core.Tests
{
    [TestClass]
    public class RequiredNonDefaultAttributeTest : BaseUnigrationTests
    {
        [TestMethod]
        [TestCategory("Unigration")]
        public async Task ValidateNoGuidFailure()
        {
            using (var srv = new TestServer(TestHostBuilder<Startup, UnitTestStartup>()))
            {
                var client = srv.CreateClient();
                var resp = await client.PostAsJsonAsync("api/v1/Test", new TestModel());
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                var s = await resp.Content.ReadAsStringAsync();
                StringAssert.Contains(s, "non-default");
                StringAssert.Contains(s, "TestGuid");
            }
        }

        [TestMethod]
        [TestCategory("Unigration")]
        public async Task ValidateEmptyGuidFailure()
        {
            using (var srv = new TestServer(TestHostBuilder<Startup, UnitTestStartup>()))
            {
                var client = srv.CreateClient();
                var resp = await client.PostAsJsonAsync("api/v1/Test", new TestModel { TestGuid = new Guid() });
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                var s = await resp.Content.ReadAsStringAsync();
                StringAssert.Contains(s, "non-default");
                StringAssert.Contains(s, "TestGuid");
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ValidateEmptyGuidFailureUnitTest()
        {
            var controller = ControllerFactory<TestController>.Create();
            var mod = new TestModel() { TestGuid = Guid.Empty };
            Assert.IsFalse(controller.TryValidateModel(mod));
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void ValidateNonEmptyGuidSuccess()
        {
            var controller = ControllerFactory<TestController>.Create();
            var result = controller.IsValid(new TestModel() { TestGuid = Guid.NewGuid() });
            Assert.IsInstanceOfType(result, typeof(OkResult));
        }

        [TestMethod]
        [TestCategory("Unigration")]
        public async Task ValidateSuccess()
        {
            using (var srv = new TestServer(TestHostBuilder<Startup, UnitTestStartup>()))
            {
                var client = srv.CreateClient();
                var resp = await client.PostAsJsonAsync("api/v1/Test", new TestModel { TestGuid = Guid.NewGuid(), strings = new[] { "Hello World" } });
                Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
                var s = await resp.Content.ReadAsStringAsync();
            }
        }


        [TestMethod]
        [TestCategory("Unigration")]
        public async Task ValidateEmptyArrayFailure()
        {
            using (var srv = new TestServer(TestHostBuilder<Startup, UnitTestStartup>()))
            {
                var client = srv.CreateClient();
                var resp = await client.PostAsJsonAsync("api/v1/Test", new TestModel { TestGuid = Guid.NewGuid(), strings = new string[0] });
                Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
                var s = await resp.Content.ReadAsStringAsync();
                StringAssert.Contains(s, "The strings field requires a non-empty value.");
            }
        }
    }
}
