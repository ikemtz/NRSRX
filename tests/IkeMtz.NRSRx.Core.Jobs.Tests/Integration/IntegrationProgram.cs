using IkeMtz.NRSRx.Jobs.Unigration;
using IkeMtz.Samples.Jobs;

namespace IkeMtz.NRSRx.Core.Jobs.Tests.Integration
{
  public class IntegrationProgram : CoreIntegrationTestJob<Program>
  {
    public IntegrationProgram(Program program, TestContext testContext) : base(program, testContext)
    {
    }
  }
}
