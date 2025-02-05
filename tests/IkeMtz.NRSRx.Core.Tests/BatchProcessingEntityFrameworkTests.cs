using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IkeMtz.NRSRx.Core.EntityFramework;
using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.NRSRx.Core.Unigration.Data;
using IkeMtz.NRSRx.Unigration.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Core.Tests
{
  [TestClass]
  public class BatchProcessingEntityFrameworkTests : BaseUnigrationTests
  {
    [TestMethod]
    [TestCategory("Unigration")]
    public async Task HandleBatchSaveChangesAsyncMultipleDbContext()
    {
      var logger = new TestContextLogger(nameof(TestAuditableDbContext), TestContext);
      var srcList = new List<CollectionGuidModel> { };
      for (var i = 0; i < 300; i++)
      {
        srcList.Add(new CollectionGuidModel { Id = Guid.NewGuid() });
      }
      var dbContextList = new List<TestAuditableContext>();
      var batchDataSaver = new BatchDataSaver<TestAuditableContext, CollectionGuidModel>();
      var result = await batchDataSaver.SaveChangesInBatchAsync(() =>
        {
          var dbContext = DbContextFactory.CreateInMemoryAuditableDbContext<TestAuditableContext>(TestContext);
          dbContextList.Add(dbContext);
          return dbContext;
        }, srcList, logger);
      Assert.AreEqual(300, result);
      Assert.AreEqual(2, dbContextList.Count);
    }
  }
}
