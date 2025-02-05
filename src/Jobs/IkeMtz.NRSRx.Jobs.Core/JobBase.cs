using System.Diagnostics.CodeAnalysis;
using IkeMtz.NRSRx.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IkeMtz.NRSRx.Jobs.Core
{
  /// <summary>
  /// Base class for defining a job in the NRSRx framework.
  /// </summary>
  /// <typeparam name="TProgram">The type of the program.</typeparam>
  public abstract class JobBase<TProgram>
      where TProgram : class
  {

    /// <summary>
    /// Gets or sets a value indicating whether the job should run continuously.
    /// </summary>
    public virtual bool RunContinously { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether parallel function processing is enabled.
    /// </summary>
    public virtual bool EnableParallelFunctionProcessing { get; set; } = false;

    /// <summary>
    /// Gets or sets the number of seconds between runs.
    /// </summary>
    public virtual int? SecsBetweenRuns { get; set; }

    /// <summary>
    /// Gets the sleep time span based on the number of seconds between runs.
    /// </summary>
    public virtual TimeSpan SleepTimeSpan => new(0, 0, SecsBetweenRuns.GetValueOrDefault());

    /// <summary>
    /// Gets or sets the job host.
    /// </summary>
    public virtual IHost JobHost { get; set; }

    /// <summary>
    /// Gets the location of the health file.
    /// </summary>
    public virtual string? HealthFileLocation { get; }

    /// <summary>
    /// Gets or sets the configuration.
    /// </summary>
    public virtual IConfiguration Configuration { get; set; }

    /// <summary>
    /// Gets the configuration for the job.
    /// </summary>
    /// <returns>The configuration.</returns>
    public virtual IConfiguration GetConfig()
    {
      return ConfigurationFactory<TProgram>.Create();
    }

    /// <summary>
    /// Runs the job asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success.</returns>
    public Task<bool> RunAsync()
    {
      _ = this.SetupHost();
      var loggerFactory = JobHost.Services.GetRequiredService<ILoggerFactory>();
      return RunFunctions(loggerFactory);
    }

    /// <summary>
    /// Runs the functions associated with the job.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to create loggers.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the functions were successful.</returns>
    public async Task<bool> RunFunctions(ILoggerFactory loggerFactory)
    {
      SecsBetweenRuns ??= Configuration.GetValue("SecsBetweenRuns", 60);
      var logger = loggerFactory.CreateLogger<JobBase<TProgram>>();
      var functions = GetFunctions(loggerFactory);
      var firstRun = true;
      var successResult = true;
      while (RunContinously || firstRun)
      {
        if (EnableParallelFunctionProcessing)
        {
          foreach (var func in functions.AsParallel())
          {
            successResult &= await ScopeFunctionAsync(loggerFactory, func);
          }
        }
        else
        {
          foreach (var func in functions)
          {
            successResult &= await ScopeFunctionAsync(loggerFactory, func);
          }
        }

        if (successResult && !string.IsNullOrWhiteSpace(HealthFileLocation))
        {
          File.WriteAllText(HealthFileLocation, DateTime.UtcNow.ToString());
        }

        firstRun = false;
        logger.LogInformation("Finished running jobs, going to sleep for {SecsBetweenRuns} seconds.", SecsBetweenRuns);

        if (RunContinously) Thread.Sleep(SleepTimeSpan);
      }
      return successResult;
    }

    /// <summary>
    /// Gets the functions to run.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>An ordered enumerable of function metadata.</returns>
    public virtual IOrderedEnumerable<FunctionMetadata> GetFunctions(ILoggerFactory? loggerFactory)
    {
      var jobLogger = loggerFactory?.CreateLogger(GetType());
      using var functionScope = JobHost.Services.CreateScope();
      var functions = functionScope.ServiceProvider.GetServices<IFunction>();
      var functionTypeName = typeof(IFunction).Name;
      var functionCount = functions.Count();
      jobLogger?.LogInformation("Found {functionCount} executable {functionTypeName} functions", functionCount, functionTypeName);
      for (var i = 0; i < functionCount; i++)
      {
        var functionName = functions.ElementAt(i);
        jobLogger?.LogInformation("[{i}] {functionName}", i, functionName);
      }
      return functions
        .Select(t =>
        {
          var type = t.GetType();
          return new FunctionMetadata
          {
            Type = type,
            Name = type.Name,
            SequencePriority = t.SequencePriority ?? 0
          };
        }).OrderByDescending(t => t.SequencePriority);
    }

    /// <summary>
    /// Scopes and runs a function asynchronously.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="func">The function metadata.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success.</returns>
    public virtual async Task<bool> ScopeFunctionAsync(ILoggerFactory loggerFactory, FunctionMetadata func)
    {
      var logger = loggerFactory.CreateLogger(func.GetType());
      var startTime = DateTime.UtcNow;
      using (logger.BeginScope("Function {functionName}", func.Name))
      {
        logger.LogInformation("Function {functionName} start time: {startTime} UTC", func.Name, startTime);
        bool result;
        try
        {
          result = await RunFunctionAsync(func, logger);
        }
        catch (Exception x)
        {
          logger.LogError(x, "An unhandled exception has occurred while executing function: {functionName}", func.Name);
          return false;
        }
        var endTime = DateTime.UtcNow;
        var durationInSecs = endTime.Subtract(startTime).TotalSeconds;
        logger.LogInformation("Function {functionName} completed time: {endTime} UTC", func.Name, endTime);
        logger.LogInformation("Function {functionName} completed in {durationInSecs} secs", func.Name, durationInSecs);
        return result;
      }
    }

    /// <summary>
    /// Sets up the host.
    /// </summary>
    /// <param name="setupMiscDependencies">An action to set up miscellaneous dependencies.</param>
    /// <returns>The configured host.</returns>
    public virtual IHost SetupHost(Action<IServiceCollection>? setupMiscDependencies = null)
    {
      if (Configuration == null || JobHost == null)
      {
        Configuration = this.GetConfig();
        JobHost = Host.CreateDefaultBuilder()
           .ConfigureServices((services) =>
           {
             SetupLogging(services);
             _ = SetupUserProvider(services);
             _ = SetupDependencies(services);
             _ = SetupFunctions(services);
             setupMiscDependencies?.Invoke(services);
           })
           .Build();
      }
      return JobHost;
    }

    /// <summary>
    /// Sets up logging.
    /// </summary>
    /// <param name="services">The service collection.</param>
    [ExcludeFromCodeCoverage]
    public virtual void SetupLogging(IServiceCollection services) { }

    /// <summary>
    /// Sets up dependencies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    [ExcludeFromCodeCoverage]
    public virtual IServiceCollection SetupDependencies(IServiceCollection services)
    {
      return services.AddSingleton(TimeProvider.System);
    }

    /// <summary>
    /// Sets up the functions.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public abstract IServiceCollection SetupFunctions(IServiceCollection services);

    /// <summary>
    /// Sets up the user provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public virtual IServiceCollection SetupUserProvider(IServiceCollection services)
    {
      return services;
    }

    /// <summary>
    /// Runs a function asynchronously.
    /// </summary>
    /// <param name="func">The function metadata.</param>
    /// <param name="logger">The logger.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success.</returns>
    public virtual async Task<bool> RunFunctionAsync(FunctionMetadata func, ILogger logger)
    {
      logger.LogInformation("Starting {functionName} function.", func.Name);
      using var functionScope = JobHost.Services.CreateScope();
      var function = functionScope.ServiceProvider.GetService(func.Type) as IFunction;
      var result = await function.RunAsync();
      if (!result)
      {
        logger.LogError("An error occurred in {functionName}.", func.Name);
      }
      logger.LogInformation("Ending {functionName} function.", func.Name);
      return result;
    }
  }
}
