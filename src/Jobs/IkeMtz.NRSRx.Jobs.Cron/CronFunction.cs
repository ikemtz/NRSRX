using IkeMtz.NRSRx.Jobs.Core;
using Microsoft.Extensions.Logging;
using NCrontab;

namespace IkeMtz.NRSRx.Jobs.Cron
{
  /// <summary>
  /// Represents a scheduled function that can be executed based on a condition.
  /// </summary>
  public abstract class CronFunction : IFunction
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CronFunction"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param> 
    /// <param name="timeProvider">The time provider instance.</param>
    public CronFunction(ILogger<CronFunction> logger, TimeProvider timeProvider)
    {
      Logger = logger;
      TimeProvider = timeProvider;
    }

    /// <summary>
    /// Gets or sets the last run date and time in UTC.
    /// </summary>
    public static DateTimeOffset? LastRunDateTimeUtc { get; set; }

    /// <summary>
    /// Gets or sets the next run date and time in UTC.
    /// </summary>
    public static DateTimeOffset? NextRunDateTimeUtc { get; set; }

    /// <summary>
    /// Gets or sets the schedule for the cron function.
    /// </summary>
    public static CrontabSchedule? Schedule { get; set; }

    /// <summary>
    /// Gets or sets the cron expression for the function.
    /// For guidance visit: https://crontab.guru/
    /// </summary>
    public abstract string CronExpression { get; set; }

    /// <inheritdoc/>
    public virtual int? SequencePriority { get; } = 100;

    /// <summary>
    /// Gets the logger for the cron function.
    /// </summary>
    public ILogger<CronFunction> Logger { get; }

    /// <summary>
    /// Gets the time provider for the cron function.
    /// </summary>
    public TimeProvider TimeProvider { get; }

    /// <summary>
    /// Executes the function asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the function executed successfully.</returns>
    public virtual async Task<bool> RunAsync()
    {
      Schedule ??= CrontabSchedule.Parse(CronExpression);
      NextRunDateTimeUtc ??= Schedule.GetNextOccurrence(LastRunDateTimeUtc.GetValueOrDefault(TimeProvider.GetUtcNow()).UtcDateTime);
      if (NextRunDateTimeUtc <= TimeProvider.GetUtcNow())
      {
        LastRunDateTimeUtc = TimeProvider.GetUtcNow();
        NextRunDateTimeUtc = Schedule.GetNextOccurrence(LastRunDateTimeUtc.Value.DateTime);
        Logger.LogInformation("Executing {FunctionName} function, at {LastRunTime}.  Next run time is: {NextRuntime}", GetType().Name, LastRunDateTimeUtc, NextRunDateTimeUtc);
        return await ExecuteAsync();
      }
      else
      {
        Logger.LogInformation("Skipping execution of {FunctionName} function, at {LastRunTime}.  Next run time is: {NextRuntime}", GetType().Name, LastRunDateTimeUtc, NextRunDateTimeUtc);
        return true;
      }
    }

    /// <summary>
    /// Executes the function logic asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the function executed successfully.</returns>
    public abstract Task<bool> ExecuteAsync();
  }
}
