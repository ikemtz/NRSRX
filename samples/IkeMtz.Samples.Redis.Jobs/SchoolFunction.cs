using IkeMtz.NRSRx.Events;
using IkeMtz.NRSRx.Events.Subscribers.Redis;
using IkeMtz.NRSRx.Jobs.Redis;
using IkeMtz.Samples.Models.V1;
using Microsoft.Extensions.Logging;

namespace IkeMtz.Samples.Redis.Jobs
{
  public class SchoolCreatedFunction : MessageFunction<SchoolCreatedFunction, School, CreatedEvent>
  {
    public SchoolCreatedFunction(ILogger<SchoolCreatedFunction> logger, RedisStreamSubscriber<School, CreatedEvent> subscriber)
      : base(logger, subscriber)
    {
    }

    public override Task HandleMessageAsync(School entity)
    {
      Logger.LogInformation("Sample handled.");
      return Task.CompletedTask;
    }
  }
}
