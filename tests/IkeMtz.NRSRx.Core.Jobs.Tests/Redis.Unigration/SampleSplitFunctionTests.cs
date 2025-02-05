using IkeMtz.NRSRx.Core.Unigration;
using IkeMtz.NRSRx.Core.Unigration.Events;
using IkeMtz.NRSRx.Events;
using IkeMtz.NRSRx.Events.Abstraction;
using IkeMtz.NRSRx.Events.Subscribers.Redis;
using IkeMtz.NRSRx.Jobs.Core;
using IkeMtz.NRSRx.Jobs.Redis;
using IkeMtz.NRSRx.Jobs.Unigration;
using IkeMtz.Samples.Models.V1;
using IkeMtz.Samples.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace IkeMtz.NRSRx.Core.Jobs.Redis.Tests.Unigration
{
  [TestClass]
  public class SampleSplitFunctionTests : BaseUnigrationTests
  {

    [TestMethod]
    [TestCategory("Unigration")]
    public async Task SampleSplitSchoolCreatedEventTest()
    {
      //arange
      var program = new SplitMessageUnigrationProgram(new SplitProgram(), TestContext)
      {
        RunContinously = false,
        SecsBetweenRuns = 1,
        EnableParallelFunctionProcessing = true,
      };

      _ = program.SetupHost(x => x.AddSingleton<SplitSchoolCreatedFunction>());
      var func = program.JobHost.Services.GetRequiredService<SplitSchoolCreatedFunction>();
      var logger = program.JobHost.Services.GetRequiredService<ILogger<SplitSchoolCreatedFunction>>();

      //act
      var messages = await program.MockSubscriber.Object.GetMessagesAsync(1);
      await func.ProcessStreamSplitBatchAsync("new", messages);

      //assert   
      program.MockSubscriber.Verify(t => t.AcknowledgeMessageAsync(It.IsAny<RedisValue>()), Times.Exactly(2));
      Assert.IsNotNull(new SplitMessage<School>());

    }
  }

  internal class SplitSchoolCreatedFunction : SplitMessageFunction<SplitSchoolCreatedFunction, School, CreatedEvent>
  {
    public SplitSchoolCreatedFunction(ILogger<SplitSchoolCreatedFunction> logger, RedisStreamSubscriber<SplitMessage<School>, CreatedEvent> subscriber)
      : base(logger, subscriber)
    {
      this.MessageBufferCount = 1;
    }
    public override Task HandleMessageAsync(SplitMessage<School> entity)
    {
      return Task.CompletedTask;
    }
  }

  internal class SplitMessageUnigrationProgram : CoreMessagingUnigrationTestJob<SplitProgram>, IJob
  {
    public Mock<RedisStreamSubscriber<SplitMessage<School>, CreatedEvent>> MockSubscriber { get; set; }
    public SplitMessageUnigrationProgram(SplitProgram program, TestContext testContext) : base(program, testContext)
    {
    }

    public override IServiceCollection SetupDependencies(IServiceCollection services)
    {
      var schools = SplitMessageFactory<School>.Create(Factories.SchoolFactory, 2);
      var (Subscriber, _) = MockRedisStreamFactory<SplitMessage<School>, CreatedEvent>.CreateSubscriber(schools);
      MockSubscriber = Subscriber;
      return services.AddSingleton(x => MockSubscriber.Object);
    }
  }
  public class SplitProgram : JobBase<SplitProgram>, IJob
  {
    public override IServiceCollection SetupFunctions(IServiceCollection services)
    {
      return services.AddSingleton<IMessageFunction, SplitSchoolCreatedFunction>();
    }
  }
}
