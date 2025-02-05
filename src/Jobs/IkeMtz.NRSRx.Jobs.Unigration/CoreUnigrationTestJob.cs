using IkeMtz.NRSRx.Core.EntityFramework;
using IkeMtz.NRSRx.Jobs.Core;
using IkeMtz.NRSRx.Unigration.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Jobs.Unigration
{
  /// <summary>
  /// Represents a core unigration test job for testing purposes.
  /// </summary>
  /// <typeparam name="TProgram">The type of the program implementing the <see cref="IJob"/> interface.</typeparam>
  /// <remarks>
  /// Initializes a new instance of the <see cref="CoreUnigrationTestJob{TProgram}"/> class.
  /// </remarks>
  /// <param name="program">The program instance.</param>
  /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
  public class CoreUnigrationTestJob<TProgram>(TProgram program, TestContext testContext) : JobBase<TProgram>
          where TProgram : class, IJob
  {
    /// <summary>
    /// Gets the program instance.
    /// </summary>
    public TProgram Program { get; } = program;

    /// <summary>
    /// Gets the MSTest <see cref="TestContext"/> for logging.
    /// </summary>
    public TestContext TestContext { get; } = testContext;

    /// <summary>
    /// Sets up the dependencies for the job.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection with dependencies set up.</returns>
    public override IServiceCollection SetupDependencies(IServiceCollection services)
    {
      return Program.SetupDependencies(services);
    }

    /// <summary>
    /// Sets up the functions for the job.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection with functions set up.</returns>
    public override IServiceCollection SetupFunctions(IServiceCollection services)
    {
      return Program.SetupFunctions(services);
    }

    /// <summary>
    /// Sets up logging for the job.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public override void SetupLogging(IServiceCollection services)
    {
      _ = services.AddSingleton(TestContext)
          .AddLogging(x => x.AddTestContext(TestContext));
    }

    /// <summary>
    /// Executes a callback on the specified database context.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <param name="callback">The callback to execute.</param>
    /// <exception cref="ArgumentNullException">Thrown if the callback is null.</exception>
    public void ExecuteOnContext<TDbContext>(Action<TDbContext> callback) where TDbContext : DbContext
    {
      _ = SetupHost();
      Program.Configuration = this.Configuration;

      callback = callback ?? throw new ArgumentNullException(nameof(callback));

      // Create a scope to obtain a reference to the database
      // context (ApplicationDbContext).
      using var scope = JobHost.Services.CreateAsyncScope();
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<TDbContext>();
      // Ensure the database is created.
      try
      {
        _ = db.Database.EnsureCreated();
      }
      catch (Exception ex)
      {
        TestContext.WriteLine($"DB Creation Exception Occurred: {ex}");
      }
      if (db is AuditableDbContext)
      {
        var dbContext = db as AuditableDbContext;
        dbContext.CurrentUserProvider = new SystemUserProvider();
      }
      TestContext.WriteLine($"Executing {nameof(ExecuteOnContext)}<{nameof(TDbContext)}> Logic");
      callback(db);
      _ = db.SaveChanges();
    }
  }
}
