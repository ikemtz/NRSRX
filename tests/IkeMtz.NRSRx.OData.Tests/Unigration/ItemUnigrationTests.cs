using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IkeMtz.NRSRx.Core.Models;
using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.Samples.OData;
using IkeMtz.Samples.OData.Data;
using IkeMtz.Samples.OData.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace IkeMtz.NRSRx.OData.Tests
{
  [TestClass]
  public class ItemUnigrationTests : BaseUnigrationTests
  {
    [TestMethod]
    [TestCategory("Unigration")]
    public async Task GetItemsTest()
    {
      var item = Factories.ItemFactory();
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>()
          .ConfigureTestServices(x =>
          {
            ExecuteOnContext<DatabaseContext>(x, db =>
            {
              _ = db.Items.Add(item);
            });
          })
       );
      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.GetStringAsync($"odata/v1/{nameof(Item)}s");
      TestContext.WriteLine($"Server Reponse: {resp}");
      Assert.IsFalse(resp.ToLower().Contains("updatedby"));
      var envelope = JsonConvert.DeserializeObject<ODataEnvelope<Item>>(resp);
      Assert.AreEqual(item.Value, envelope.Value.First().Value);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task GetGroupByItemsTest()
    {
      var item = Factories.ItemFactory();
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>()
          .ConfigureTestServices(x =>
          {
            ExecuteOnContext<DatabaseContext>(x, db =>
            {
              _ = db.Items.Add(item);
            });
          })
       );
      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.GetStringAsync($"odata/v1/{nameof(Item)}s?$apply=groupby(({nameof(item.Value)},{nameof(item.Id)}),aggregate(id with countdistinct as total))");
      TestContext.WriteLine($"Server Reponse: {resp}");
      Assert.IsFalse(resp.ToLower().Contains("updatedby"));
      StringAssert.Contains(resp, item.Id.ToString());
      StringAssert.Contains(resp, item.Value);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task GetItemsWithExpansionTest()
    {
      var item = Factories.ItemFactory();
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>()
          .ConfigureTestServices(x =>
          {
            ExecuteOnContext<DatabaseContext>(x, db =>
            {
              _ = db.Items.Add(item);
            });
          })
       );
      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.GetStringAsync($"odata/v1/{nameof(Item)}s?$count=true&$expand={nameof(SubItemA)}s,{nameof(SubItemB)}s");
      TestContext.WriteLine($"Server Reponse: {resp}");
      Assert.IsFalse(resp.ToLower().Contains("updatedby"));
      var envelope = JsonConvert.DeserializeObject<ODataEnvelope<Item>>(resp);
      Assert.AreEqual(item.Value, envelope.Value.First().Value);
      Assert.AreEqual(item.SubItemAs.First().Id, envelope.Value.First().SubItemAs.First().Id);
      Assert.AreEqual(item.SubItemBs.First().Id, envelope.Value.First().SubItemBs.First().Id);
      Assert.AreEqual(item.SubItemBs.First().Id, envelope.Value.First().SubItemBs.First().Id);
      Assert.IsFalse(envelope.Value.First().SubItemCs.Any());
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task GetMaxErrorItemsTest()
    {
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>());
      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.GetAsync($"odata/v1/{nameof(Item)}s?$top=5000&$count=true");
      var data = await resp.Content.ReadAsStringAsync();
      TestContext.WriteLine($"Server Reponse: {data}");
      Assert.IsTrue(data.Contains("The limit of '100'"));
      Assert.IsTrue(data.Contains("The value from the incoming request is '5000'"));
      Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task GetMaxItemsTest()
    {
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>());
      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.GetAsync($"odata/v1/{nameof(Item)}s/nolimit?$top=500&$count=true");
      var data = await resp.Content.ReadAsStringAsync();
      TestContext.WriteLine($"Server Reponse: {data}");
      Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task DeleteItemsTest()
    {
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>());
      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.DeleteAsync($"odata/v1/{nameof(Item)}s/{Guid.NewGuid()}");
      Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    [Ignore] //NOSONAR
    public async Task GetMetaData()
    {
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationTestStartup>());
      var client = srv.CreateClient();
      var resp = await client.GetAsync($"$odata");
      var data = await resp.Content.ReadAsStringAsync();
      TestContext.WriteLine($"Server Reponse: {data}");
      Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
    }
  }
}
