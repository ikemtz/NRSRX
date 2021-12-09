using System.Threading.Tasks;
using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.NRSRx.Core.Unigration.Events;
using IkeMtz.NRSRx.Events;
using IkeMtz.Samples.Events.Redis;
using IkeMtz.Samples.Events.Tests.Integration;
using IkeMtz.Samples.Models.V1;
using IkeMtz.Samples.Tests;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace IkeMtz.Samples.Events.Tests.Unigration
{
  [TestClass]
  public partial class CoursesTests : BaseUnigrationTests
  {
    [TestMethod]
    [TestCategory("Unigration")]
    public async Task CreateCoursesTest()
    {
      var mockPublisher = MockRedisStreamFactory<Course, CreatedEvent>.CreatePublisher();
      var item = Factories.CourseFactory();
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationEventsTestStartup>().ConfigureServices(x =>
      {
        _ = x.AddSingleton(mockPublisher.Object);
      }));

      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.PostAsJsonAsync($"api/v1/{nameof(Course)}s.json", item);
      _ = resp.EnsureSuccessStatusCode();
      mockPublisher.Verify(t => t.PublishAsync(It.Is<Course>(t => t.Id == item.Id)), Times.Once);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task UpdateCoursesTest()
    {
      var mockPublisher = MockRedisStreamFactory<Course, UpdatedEvent>.CreatePublisher();
      var item = Factories.CourseFactory();
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationEventsTestStartup>().ConfigureServices(x =>
      {
        _ = x.AddSingleton(mockPublisher.Object);
      }));

      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.PutAsJsonAsync($"api/v1/{nameof(Course)}s.json?id={item.Id}", item);
      _ = resp.EnsureSuccessStatusCode();
      mockPublisher.Verify(t => t.PublishAsync(It.Is<Course>(t => t.Id == item.Id)), Times.Once);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task DeleteCoursesTest()
    {
      var mockPublisher = MockRedisStreamFactory<Course, DeletedEvent>.CreatePublisher();
      var item = Factories.CourseFactory();
      using var srv = new TestServer(TestHostBuilder<Startup, UnigrationEventsTestStartup>().ConfigureServices(x =>
      {
        _ = x.AddSingleton(mockPublisher.Object);
      }));

      var client = srv.CreateClient();
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.DeleteAsync($"api/v1/{nameof(Course)}s.json?id={item.Id}");
      _ = resp.EnsureSuccessStatusCode();
      mockPublisher.Verify(t => t.PublishAsync(It.Is<Course>(t => t.Id == item.Id)), Times.Once);
    }
  }
}