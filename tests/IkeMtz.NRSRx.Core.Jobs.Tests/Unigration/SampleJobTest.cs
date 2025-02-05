using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.NRSRx.Jobs.Tests.Unigration;
using IkeMtz.Samples.Data;
using IkeMtz.Samples.Jobs;
using IkeMtz.Samples.Tests;
using Microsoft.EntityFrameworkCore;

namespace IkeMtz.NRSRx.Core.Jobs.Tests.Unigration
{
  [TestClass]
  public class SampleJobTests : BaseUnigrationTests
  {
    [TestMethod]
    [TestCategory("Unigration")]
    public async Task SampleJobTest()
    {
      //arange
      var program = new UnigrationProgram(new Program(), TestContext);
      program.ExecuteOnContext<DatabaseContext>(x => x.Courses.Add(Factories.CourseFactory()));

      //act
      await program.RunAsync();

      //assert
      program.ExecuteOnContext<DatabaseContext>(async x =>
      {
        var schoolCount = await x.Schools.CountAsync();
        Assert.AreEqual(0, schoolCount);
        var courseCount = await x.Courses.CountAsync();
        Assert.AreEqual(2, courseCount);
      });
    }
  }
}
