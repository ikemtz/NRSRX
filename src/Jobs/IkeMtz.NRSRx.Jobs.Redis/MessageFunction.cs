using IkeMtz.NRSRx.Core.Models;
using IkeMtz.NRSRx.Events;
using IkeMtz.NRSRx.Events.Subscribers.Redis;
using IkeMtz.NRSRx.Jobs.Core;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace IkeMtz.NRSRx.Jobs.Redis
{
  /// <summary>
  /// Abstract base class for message functions handling entities with a <see cref="Guid"/> identifier.
  /// </summary>
  /// <typeparam name="TMessageFunction">The type of the message function.</typeparam>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <typeparam name="TEvent">The type of the event.</typeparam>
  /// <remarks>
  /// Initializes a new instance of the <see cref="MessageFunction{TMessageFunction, TEntity, TEvent}"/> class.
  /// </remarks>
  /// <param name="logger">The logger instance.</param>
  /// <param name="subscriber">The Redis stream subscriber.</param>
  public abstract class MessageFunction<TMessageFunction, TEntity, TEvent>(ILogger<TMessageFunction> logger, RedisStreamSubscriber<TEntity, TEvent, Guid> subscriber) : MessageFunction<TMessageFunction, TEntity, TEvent, Guid>(logger, subscriber)
      where TMessageFunction : IMessageFunction
      where TEntity : class, IIdentifiable<Guid>
      where TEvent : EventType, new()
  {
  }

  /// <summary>
  /// Abstract base class for message functions handling entities.
  /// </summary>
  /// <typeparam name="TMessageFunction">The type of the message function.</typeparam>
  /// <typeparam name="TEntity">The type of the entity.</typeparam>
  /// <typeparam name="TEvent">The type of the event.</typeparam>
  /// <typeparam name="TIdentityType">The type of the entity identifier.</typeparam>
  /// <remarks>
  /// Initializes a new instance of the <see cref="MessageFunction{TMessageFunction, TEntity, TEvent, TIdentityType}"/> class.
  /// </remarks>
  /// <param name="logger">The logger instance.</param>
  /// <param name="subscriber">The Redis stream subscriber.</param>
  public abstract class MessageFunction<TMessageFunction, TEntity, TEvent, TIdentityType>(ILogger<TMessageFunction> logger, RedisStreamSubscriber<TEntity, TEvent, TIdentityType> subscriber) : IMessageFunction
    where TMessageFunction : IMessageFunction
    where TIdentityType : IComparable
    where TEntity : class, IIdentifiable<TIdentityType>
    where TEvent : EventType, new()
  {
    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    public ILogger<TMessageFunction> Logger { get; } = logger;

    /// <summary>
    /// Gets or sets the Redis stream subscriber.
    /// </summary>
    public virtual RedisStreamSubscriber<TEntity, TEvent, TIdentityType> Subscriber { get; set; } = subscriber;

    /// <summary>
    /// Gets or sets the message buffer count.
    /// </summary>
    public virtual int MessageBufferCount { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether to enable pending message processing.
    /// Default value is true.
    /// </summary>
    public virtual bool EnablePendingMsgProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets the sequence priority.
    /// Higher priority functions get run first (ordered by descending).
    /// </summary>
    public virtual int? SequencePriority { get; set; } = null;

    /// <summary>
    /// Runs the message function asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous run operation. The task result contains a boolean indicating success.</returns>
    public virtual async Task<bool> RunAsync()
    {
      await LogStreamHealthInformationAsync();
      await ProcessStreamsAsync("new", Subscriber.GetMessagesAsync);

      if (EnablePendingMsgProcessing)
      {
        await ProcessStreamsAsync("pending", Subscriber.GetPendingMessagesAsync);
      }
      await Subscriber.DeleteIdleConsumersAsync();
      Subscriber.ConsumerName = Guid.NewGuid().ToString("N");
      return true;
    }

    /// <summary>
    /// Processes the streams asynchronously.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <param name="getMessageFunction">The function to get messages.</param>
    /// <returns>A task that represents the asynchronous process operation.</returns>
    public virtual async Task ProcessStreamsAsync(string messageType, Func<int?, Task<IEnumerable<(RedisValue Id, TEntity Entity)>>> getMessageFunction)
    {
      Logger.LogInformation("Pulling {MessageBufferCount} {messageType} messages from queue.", MessageBufferCount, messageType);
      var messages = await getMessageFunction(MessageBufferCount);
      var messageCount = messages.Count();
      Logger.LogInformation("Received {messageCount} {messageType} messages from queue.", messageCount, messageType);
      var processedMessageCount = 0;
      foreach (var (redisId, entity) in messages)
      {
        var id = entity.Id;
        try
        {
          Logger.LogInformation("Handling {messageType} message with entity id: {id} ", messageType, id);
          await HandleMessageAsync(entity);
          Logger.LogInformation("Handled {messageType} message with entity id: {id} ", messageType, id);
          await Subscriber.AcknowledgeMessageAsync(redisId);
          processedMessageCount++;
        }
        catch (Exception x)
        {
          Logger.LogError(x, "An error occurred while handling {messageType} message with entity id: {id} ", messageType, id);
        }
      }
      Logger.LogInformation("Processed {processedMessageCount} {messageType} messages from queue.", processedMessageCount, messageType);
    }

    /// <summary>
    /// Logs the stream health information asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous log operation.</returns>
    public virtual async Task LogStreamHealthInformationAsync()
    {
      var result = await Subscriber.GetStreamInfoAsync();
      if (result != null) // This can happen in mocked scenarios
      {
        Logger.LogInformation("Consumer Group {ConsumerGroupName}, Msg Count: {MsgCount}, Acknowledged Msg Count: {AcknowledgedMsgCount}, Consumer Count: {SubscriberCount}, Pending Count: {PendingMsgCount}, Dead Letter Msg Count: {DeadLetterMsgCount}",
          result.StreamKey, result.MsgCount, result.AcknowledgedMsgCount, result.SubscriberCount, result.PendingMsgCount, result.DeadLetterMsgCount);
      }
      else
      {
        Logger.LogWarning("Stream Info is not available.");
      }
    }

    /// <summary>
    /// Handles the message asynchronously.
    /// </summary>
    /// <param name="entity">The entity to handle.</param>
    /// <returns>A task that represents the asynchronous handle operation.</returns>
    public abstract Task HandleMessageAsync(TEntity entity);
  }
}
