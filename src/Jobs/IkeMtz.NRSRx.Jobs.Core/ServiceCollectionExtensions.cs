using IkeMtz.NRSRx.Jobs.Core;

namespace Microsoft.Extensions.DependencyInjection
{
  /// <summary>
  /// Provides extension methods for adding functions to the service collection.
  /// </summary>
  public static class ServiceCollectionExtensions
  {
    /// <summary>
    /// Adds a function to the service collection.
    /// </summary>
    /// <typeparam name="TFunction">The type of the function to add.</typeparam>
    /// <param name="services">The service collection to add the function to.</param>
    /// <returns>The service collection with the function added.</returns>
    public static IServiceCollection AddFunction<TFunction>(this IServiceCollection services)
    where TFunction : class, IFunction
    {
      services = services
        .AddScoped<TFunction>()
        .AddScoped<IFunction, TFunction>();
      return services;
    }

    /// <summary>
    /// Adds a message function to the service collection.
    /// </summary>
    /// <typeparam name="TFunction">The type of the message function to add.</typeparam>
    /// <param name="services">The service collection to add the message function to.</param>
    /// <returns>The service collection with the message function added.</returns>
    [Obsolete("Use AddFunction<TFunction> instead.")]
    public static IServiceCollection AddMessageFunction<TFunction>(this IServiceCollection services)
    where TFunction : class, IMessageFunction
    {
      return services.AddFunction<TFunction>();
    }
  }
}
