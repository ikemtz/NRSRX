using System.Diagnostics.CodeAnalysis;
using IkeMtz.NRSRx.Jobs.Core;
using IkeMtz.NRSRx.Core.Models;
using IkeMtz.NRSRx.Events;
using IkeMtz.NRSRx.Events.Abstraction;
using IkeMtz.NRSRx.Events.Subscribers.Redis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace IkeMtz.NRSRx.Jobs.Redis
{
  /// <summary>
  /// This message function is geared towards supporting the Fanout pattern.
  /// </summary>
  /// <typeparam name="TSplitMessageFunction">The type of the split message function.</typeparam>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <typeparam name="TEvent">The type of the event.</typeparam>
  public abstract class SplitMessageFunction<TSplitMessageFunction, TEntity, TEvent>(ILogger<TSplitMessageFunction> logger,
      RedisStreamSubscriber<SplitMessage<TEntity>, TEvent> subscriber)
      : MessageFunction<TSplitMessageFunction, SplitMessage<TEntity>, TEvent, Guid>(logger, subscriber)
        where TSplitMessageFunction : IMessageFunction
        where TEntity : class, IIdentifiable<Guid>
        where TEvent : EventType, new()
  {
    /// <summary>
    /// Gets or sets a value indicating whether to automatically delete split progress data.
    /// </summary>
    public bool AutoDeleteSplitProgressData { get; set; } = true;

    /// <summary>
    /// Constant for passed messages.
    /// </summary>
    public const string PASS = "Passed";

    /// <summary>
    /// Constant for failed messages.
    /// </summary>
    public const string FAIL = "Failed";

    /// <summary>
    /// Processes the streams asynchronously.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="getMessageFunction">The function to get messages.</param>
    [ExcludeFromCodeCoverage]
    public override async Task ProcessStreamsAsync(string messageType, Func<int?,
      Task<IEnumerable<(RedisValue Id, SplitMessage<TEntity> Entity)>>> getMessageFunction)
    {
      IEnumerable<(RedisValue Id, SplitMessage<TEntity> Entity)> messages;
      do
      {
        messages = await getMessageFunction(MessageBufferCount);
        await ProcessStreamSplitBatchAsync(messageType, messages);
      }
      while (messages.Any());
    }

    /// <summary>
    /// Processes a batch of split messages asynchronously.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="messages">The messages to process.</param>
    public virtual async Task ProcessStreamSplitBatchAsync(string messageType, IEnumerable<(RedisValue Id, SplitMessage<TEntity> Entity)> messages)
    {
      Logger.LogInformation("Pulling {MessageBufferCount} {messageType} messages from queue.", MessageBufferCount, messageType);
      Logger.LogInformation("Received {messageCount} {messageType} messages from queue.", messages.Count(), messageType);
      int processedMessageCount = 0;
      foreach (var (redisId, entity) in messages)
      {
        var id = entity.Id;
        try
        {
          Logger.LogInformation("Handling {messageType} message with entity id: {id} ", messageType, id);
          await HandleMessageAsync(entity);
          Logger.LogInformation("Handled {messageType} message with entity id: {id} ", messageType, id);
          await Subscriber.AcknowledgeMessageAsync(redisId);
          await NotifySplitProgress(entity, true);
          processedMessageCount++;
        }
        catch (Exception x)
        {
          Logger.LogError(x, "An error occurred while handling {messageType} message with entity id: {id} ", messageType, id);
          await NotifySplitProgress(entity, false);
        }
      }
      Logger.LogInformation("Processed {processedMessageCount} {messageType} messages from queue.", processedMessageCount, messageType);
    }

    /// <summary>
    /// Notifies the progress of split messages.
    /// </summary>
    /// <param name="entity">The split message entity.</param>
    /// <param name="isSuccess">Indicates whether the message was processed successfully.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the progress update.</returns>
    public virtual async Task<SplitMessageProgressUpdate> NotifySplitProgress(SplitMessage<TEntity> entity, bool isSuccess)
    {
      var passFail = isSuccess ? PASS : FAIL;
      var incrementKey = $"{Subscriber.StreamKey}:{entity.Id}";
      await Subscriber.Database.HashIncrementAsync(incrementKey, passFail, 1);
      var result = await Subscriber.Database.HashGetAllAsync(incrementKey);

      var progressUpdate = ConvertHashSet(result, entity.TaskCount);
      if (entity.TaskCount <= progressUpdate.Passed + progressUpdate.Failed)
      {
        if (AutoDeleteSplitProgressData)
        {
          await Subscriber.Database.KeyDeleteAsync(incrementKey);
        }
        await NotifySplitCompletion(entity);
      }
      return progressUpdate;
    }

    /// <summary>
    /// Converts a hash set to a split message progress update.
    /// </summary>
    /// <param name="result">The hash set result.</param>
    /// <param name="totalMessages">The total number of messages.</param>
    /// <returns>The split message progress update.</returns>
    public virtual SplitMessageProgressUpdate ConvertHashSet(HashEntry[] result, int totalMessages)
    {
      var passVal = result.FirstOrDefault(t => t.Name == PASS);
      var failVal = result.FirstOrDefault(t => t.Name == FAIL);
      return new SplitMessageProgressUpdate()
      {
        Total = totalMessages,
        Passed = Convert.ToInt32(passVal.Value),
        Failed = Convert.ToInt32(failVal.Value),
      };
    }

    /// <summary>
    /// Notifies the completion of split messages.
    /// </summary>
    /// <param name="entity">The split message entity.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public virtual Task NotifySplitCompletion(SplitMessage<TEntity> entity)
    {
      return Task.CompletedTask;
    }
  }
}
