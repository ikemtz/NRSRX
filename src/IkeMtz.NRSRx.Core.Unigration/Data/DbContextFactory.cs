using IkeMtz.NRSRx.Core.EntityFramework;
using IkeMtz.NRSRx.Unigration.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Core.Unigration.Data
{
  /// <summary>
  /// Factory for creating in-memory DbContext instances for testing purposes.
  /// </summary>
  public static class DbContextFactory
  {
    /// <summary>
    /// Creates an in-memory auditable DbContext instance.
    /// </summary>
    /// <typeparam name="TAuditableDbContext">The type of the auditable DbContext.</typeparam>
    /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
    /// <returns>An instance of <typeparamref name="TAuditableDbContext"/>.</returns>
    public static TAuditableDbContext CreateInMemoryAuditableDbContext<TAuditableDbContext>(TestContext testContext)
        where TAuditableDbContext : AuditableDbContext
    {
      var options = CreateDbContextOptions<TAuditableDbContext>(testContext);
      var constructor = typeof(TAuditableDbContext)
          .GetConstructor([options.GetType(), typeof(SystemUserProvider)]);
      return (TAuditableDbContext)constructor.Invoke([options, new SystemUserProvider()]);
    }

    /// <summary>
    /// Creates an in-memory DbContext instance.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
    /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
    /// <returns>An instance of <typeparamref name="TDbContext"/>.</returns>
    public static TDbContext CreateInMemoryDbContext<TDbContext>(TestContext testContext)
        where TDbContext : DbContext
    {
      var options = CreateDbContextOptions<TDbContext>(testContext);
      var constructor = typeof(TDbContext)
          .GetConstructor([options.GetType()]);
      return (TDbContext)constructor.Invoke([options]);
    }

    /// <summary>
    /// Creates DbContext options for in-memory testing.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the DbContext.</typeparam>
    /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
    /// <returns>The DbContext options.</returns>
    public static DbContextOptions<TDbContext> CreateDbContextOptions<TDbContext>(TestContext testContext)
        where TDbContext : DbContext
    {
      var builder = new DbContextOptionsBuilder<TDbContext>();
      _ = builder.ConfigureTestDbContextOptions(testContext)
        .AddInterceptors(new CalculatableTestInterceptor(),
          new AuditableTestInterceptor(new SystemUserProvider()));
      return builder.Options;
    }

    /// <summary>
    /// Configures the DbContext options for in-memory testing.
    /// </summary>
    /// <param name="optionsBuilder">The options builder.</param>
    /// <param name="testContext">The MSTest <see cref="TestContext"/> for logging.</param>
    /// <returns>The configured options builder.</returns>
    public static DbContextOptionsBuilder ConfigureTestDbContextOptions(this DbContextOptionsBuilder optionsBuilder, TestContext testContext)
    {
      return optionsBuilder
         .UseInMemoryDatabase($"InMemoryDbForTesting-{testContext.TestName}",
            x => x.EnableNullChecks(true))
         .EnableSensitiveDataLogging()
         .EnableDetailedErrors()
         .LogTo(testContext.WriteLine)
         .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
         .UseLoggerFactory(new LoggerFactory([new TestContextLoggerProvider(testContext)]));
    }
  }
}
