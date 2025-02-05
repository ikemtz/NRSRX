using IkeMtz.NRSRx.Jobs.Core;
using IkeMtz.Samples.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IkeMtz.Samples.Jobs
{
  internal class SchoolFunction : IFunction
  {
    public ILogger Logger { get; }
    public int? SequencePriority => null;
    public DatabaseContext DatabaseContext { get; }

    public SchoolFunction(ILogger<SchoolFunction> logger, DatabaseContext databaseContext)
    {
      Logger = logger;
      DatabaseContext = databaseContext;
    }

    public async Task<bool> RunAsync()
    {
      Logger.LogInformation("Getting school record count from database");
      var count = await DatabaseContext.Schools.CountAsync();
      Logger.LogInformation("Schools found: {count}", count);
      return true;
    }
  }
}
