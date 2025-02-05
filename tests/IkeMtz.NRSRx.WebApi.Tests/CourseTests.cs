using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.Samples.Data;
using IkeMtz.Samples.Models.V1;
using IkeMtz.Samples.Tests;
using IkeMtz.Samples.WebApi;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace IkeMtz.NRSRx.WebApi.Tests
{
  [TestClass]
  public class CourseTests : BaseUnigrationTests
  {
    [TestMethod]
    [TestCategory("Unigration")]
    public async Task GetCourseTestAsync()
    {
      var item = Factories.CourseFactory();
      using var srv = new TestServer(TestWebHostBuilder<Startup, UnigrationTestStartup>()
        .ConfigureTestServices(x =>
        {
          ExecuteOnContext<DatabaseContext>(x, db =>
          {
            _ = db.Courses.Add(item);
          });
        }));
      var client = srv.CreateClient(TestContext);
      GenerateAuthHeader(client, GenerateTestToken(new[] { new Claim("MyTestClaim", Guid.NewGuid().ToString()) }));
      //Get 
      var resp = await client.GetAsync($"api/v1/{nameof(Course)}s.json?id={item.Id}");
      var httpCourse = await DeserializeResponseAsync<Course>(resp);
      Assert.IsNotNull(httpCourse);
      Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);
      Assert.AreEqual(item.Title, httpCourse.Title);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task SaveCourseTest()
    {
      var item = Factories.CourseFactory();
      using var srv = new TestServer(TestWebHostBuilder<Startup, UnigrationTestStartup>());
      var client = srv.CreateClient(TestContext);
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.PostAsJsonAsync($"api/v1/{nameof(Course)}s.json", item);
      _ = resp.EnsureSuccessStatusCode();
      var httpCourse = await DeserializeResponseAsync<Course>(resp);
      Assert.IsNotNull(httpCourse);
      Assert.AreEqual("IntegrationTester@email.com", httpCourse.CreatedBy);

      var dbContext = srv.GetDbContext<DatabaseContext>();
      var dbCourses = await dbContext.Courses.ToListAsync();

      Assert.AreEqual(1, dbCourses.Count);
      var dbCourse = dbCourses.FirstOrDefault();
      Assert.IsNotNull(dbCourse);
      Assert.AreEqual(httpCourse.CreatedOnUtc, dbCourse.CreatedOnUtc);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    [ExpectedException(typeof(JsonReaderException))]
    public async Task SaveCourseJsonReaderExceptionsTest()
    {
      var item = Factories.CourseFactory();
      using var srv = new TestServer(TestWebHostBuilder<Startup, UnigrationTestStartup>());
      var client = srv.CreateClient(TestContext);
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.PostAsJsonAsync($"api/v1/{nameof(Course)}s.xml", item);
      _ = resp.EnsureSuccessStatusCode();
      _ = await DeserializeResponseAsync<Course>(resp);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task UpdateCourseTest()
    {
      var originalCourse = Factories.CourseFactory();
      using var srv = new TestServer(TestWebHostBuilder<Startup, UnigrationTestStartup>()
        .ConfigureTestServices(x =>
        {
          ExecuteOnContext<DatabaseContext>(x, db =>
          {
            _ = db.Courses.Add(originalCourse);
          });
        }));
      var client = srv.CreateClient(TestContext);
      GenerateAuthHeader(client, GenerateTestToken());

      var updatedCourse = JsonClone(originalCourse);
      updatedCourse.Title = Guid.NewGuid().ToString()[..6];

      var resp = await client.PutAsJsonAsync($"api/v1/{nameof(Course)}s.json?id={updatedCourse.Id}", updatedCourse);
      _ = resp.EnsureSuccessStatusCode();
      var httpUpdatedCourse = await DeserializeResponseAsync<Course>(resp);
      Assert.IsNotNull(httpUpdatedCourse);
      Assert.AreEqual("IntegrationTester@email.com", httpUpdatedCourse.UpdatedBy);
      Assert.AreEqual(1, httpUpdatedCourse.UpdateCount);
      Assert.AreEqual(updatedCourse.Title, httpUpdatedCourse.Title);
      Assert.IsNull(updatedCourse.UpdatedOnUtc);
      Assert.IsNotNull(httpUpdatedCourse.UpdatedOnUtc);

      var dbContext = srv.GetDbContext<DatabaseContext>();
      var dbCourses = await dbContext.Courses.ToListAsync();

      Assert.AreEqual(1, dbCourses.Count);
      var updatedDbCourse = dbCourses.FirstOrDefault();
      Assert.IsNotNull(updatedDbCourse);
      Assert.IsNotNull(updatedDbCourse.UpdatedOnUtc);
      Assert.AreEqual(httpUpdatedCourse.UpdatedOnUtc, updatedDbCourse.UpdatedOnUtc);
    }

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task DeleteCourseTest()
    {
      var item = Factories.CourseFactory();
      using var srv = new TestServer(TestWebHostBuilder<Startup, UnigrationTestStartup>()
        .ConfigureTestServices(x =>
        {
          ExecuteOnContext<DatabaseContext>(x, db =>
          {
            _ = db.Courses.Add(item);
          });
        }));
      var client = srv.CreateClient(TestContext);
      GenerateAuthHeader(client, GenerateTestToken());

      var resp = await client.DeleteAsync($"api/v1/{nameof(Course)}s.json?id={item.Id}");
      _ = resp.EnsureSuccessStatusCode();
      var httpUpdatedCourse = await DeserializeResponseAsync<Course>(resp);
      Assert.IsNotNull(httpUpdatedCourse);
      Assert.IsNull(httpUpdatedCourse.UpdatedBy);

      var dbContext = srv.GetDbContext<DatabaseContext>();
      var dbCourses = await dbContext.Courses.ToListAsync();

      Assert.AreEqual(0, dbCourses.Count);
    }
  }
}
