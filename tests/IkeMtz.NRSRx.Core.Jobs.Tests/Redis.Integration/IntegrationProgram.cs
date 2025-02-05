using IkeMtz.NRSRx.Jobs.Core;
using IkeMtz.NRSRx.Jobs.Unigration;
using IkeMtz.Samples.Redis.Jobs;

namespace IkeMtz.NRSRx.Core.Jobs.Tests.Redis.Integration
{
  public class IntegrationProgram : CoreMessagingIntegrationTestJob<Program>, IJob
  {
    public IntegrationProgram(Program program, TestContext testContext) : base(program, testContext)
    {
    }
  }
}
