using IkeMtz.NRSRx.Core.Unigration.Events;
using IkeMtz.NRSRx.Events;
using IkeMtz.NRSRx.Events.Abstraction;
using IkeMtz.NRSRx.Events.Subscribers.Redis;
using IkeMtz.NRSRx.Jobs.Core;
using IkeMtz.NRSRx.Jobs.Unigration;
using IkeMtz.Samples.Models.V1;
using IkeMtz.Samples.Redis.Jobs;
using IkeMtz.Samples.Tests;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace IkeMtz.NRSRx.Core.Jobs.Redis.Tests.Unigration
{
  internal class UnigrationProgram : CoreMessagingUnigrationTestJob<Program>, IJob
  {
    public Mock<RedisStreamSubscriber<School, CreateEvent>> SchoolCreatedSubMock { get; set; }
    public Mock<RedisStreamSubscriber<SplitMessage<School>, UpdatedEvent>> SchoolUpdatedSplitSubMock { get; set; }
    public UnigrationProgram(Program program, TestContext testContext) : base(program, testContext)
    {
    }

    public override IServiceCollection SetupDependencies(IServiceCollection services)
    {
      var schools = new[] {
        Factories.SchoolFactory(),
        Factories.SchoolFactory(),
      };
      var (createdSubscriber, _) = MockRedisStreamFactory<School, CreateEvent>.CreateSubscriber(schools);
      SchoolCreatedSubMock = createdSubscriber;
      var firstRun = true;
      var (updatedSubscriber, _) = MockRedisStreamFactory<SplitMessage<School>, UpdatedEvent>.CreateSubscriber(() =>
      {
        if (firstRun)
        {
          firstRun = false;
          return SplitMessage<School>.FromCollection(new[] { Factories.SchoolFactory() }, "unigration test", "NRSRx test user");
        }
        else
        {
          return Array.Empty<SplitMessage<School>>();
        }
      });
      SchoolUpdatedSplitSubMock = updatedSubscriber;

      return services
         .AddSingleton(x => SchoolCreatedSubMock.Object)
         .AddSingleton(x => SchoolUpdatedSplitSubMock.Object);
    }
  }
}
