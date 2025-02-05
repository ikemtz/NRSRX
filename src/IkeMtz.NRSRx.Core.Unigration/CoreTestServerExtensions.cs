using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using IkeMtz.NRSRx.Core.EntityFramework;
using IkeMtz.NRSRx.Core.Unigration.Data;
using IkeMtz.NRSRx.Core.Web;
using IkeMtz.NRSRx.Unigration.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IkeMtz.NRSRx.Core.Unigration
{
  /// <summary>
  /// Provides extension methods for setting up test servers and services in integration tests.
  /// </summary>
  public static class CoreTestServerExtensions
  {
    /// <summary>
    /// The issuer for JWT tokens used in integration tests.
    /// </summary>
    public const string JwtTokenIssuer = "UnigrationTestOAuthServer";

    /// <summary>
    /// The audience for JWT tokens used in integration tests.
    /// </summary>
    public const string JwtTokenAud = "@IkeMtz";

    /// <summary>
    /// Sets up test authentication using JWT bearer tokens.
    /// </summary>
    /// <param name="builder">The authentication builder.</param>
    /// <param name="Configuration">The configuration.</param>
    /// <param name="testContext">The test context.</param>
    public static void SetupTestAuthentication(this AuthenticationBuilder builder, IConfiguration Configuration, TestContext testContext)
    {
      _ = builder
      .AddJwtBearer(options =>
      {
        options.Events = new JwtBearerEvents()
        {
          OnMessageReceived = x =>
              {
                var bearer = x.Request.Headers.Authorization.ToString().Split(" ").Last();
                if (!string.IsNullOrWhiteSpace(bearer))
                {
                  var token = new JwtSecurityToken(bearer);
                  var identity = new ClaimsIdentity(token.Claims, "IntegrationTest", JwtRegisteredClaimNames.Email, "role");
                  x.Principal = new ClaimsPrincipal([identity]);
                  x.Success();
                }
                else
                {
                  testContext?.WriteLine("*** UnauthorizedAccessException ***");
                  testContext?.WriteLine(" No Authorization header provided. ");
                  x.Fail(new UnauthorizedAccessException("No Authorization header provided."));
                }
                return Task.CompletedTask;
              },
          OnAuthenticationFailed = x =>
              {
                testContext?.WriteLine("*** Authentication Failed ***");
                testContext?.WriteLine($"Exception: {x.Exception?.Message}");
                testContext?.WriteLine($"Failure: {x.Result?.Failure?.Message}");
                x.Request?.Headers?.ToList().ForEach(header => testContext?.WriteLine($"Header - {header.Key}: {header.Value}"));
                return Task.CompletedTask;
              }
        };
        options.Authority = Configuration.GetValue<string>("IdentityProvider") ?? JwtTokenIssuer;
        options.Audience = JwtTokenAud;
        options.RequireHttpsMetadata = false;
        options.IncludeErrorDetails = true;
      });
    }

    /// <summary>
    /// Sets up the test database context.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection SetupTestDbContext<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
      // Build the service provider.
      var serviceProvider = services
          .AddEntityFrameworkInMemoryDatabase()
          .AddScoped<ILoggerFactory>(provider => new LoggerFactory([
                new TestContextLoggerProvider(provider.GetService<TestContext>()) ]))
          .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
          .AddSingleton<ICurrentUserProvider, HttpUserProvider>()
          .BuildServiceProvider();
      var testContext = serviceProvider.GetService<TestContext>();
      return services.AddDbContext<TDbContext>(options =>
      {
        _ = options
              .ConfigureTestDbContextOptions(testContext)
              .LogTo(testContext.WriteLine);
        if (!typeof(TDbContext).IsAssignableFrom(typeof(AuditableDbContext)))
        {
          var currentUserProvider = serviceProvider.GetService<ICurrentUserProvider>();
          _ = options.AddInterceptors(new CalculatableTestInterceptor(), new AuditableTestInterceptor(currentUserProvider));
        }
      }, ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Builds a SignalR connection for the specified hub endpoint.
    /// </summary>
    /// <param name="srv">The test server.</param>
    /// <param name="hubEndpoint">The hub endpoint.</param>
    /// <param name="accessToken">The access token.</param>
    /// <returns>The SignalR connection.</returns>
    public static HubConnection BuildSignalrConnection(this TestServer srv, string hubEndpoint, string accessToken)
    {
      return new HubConnectionBuilder()
        .WithUrl($"{srv.BaseAddress}{hubEndpoint}",
        hubConnectionOptions =>
        {
          hubConnectionOptions.HttpMessageHandlerFactory = _ => srv.CreateHandler();
          hubConnectionOptions.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
        })
        .Build();
    }

    /// <summary>
    /// Gets the XML comments file path for the specified startup assembly.
    /// </summary>
    /// <param name="startupAssembly">The startup assembly.</param>
    /// <returns>The XML comments file path.</returns>
    public static string GetXmlCommentsFile(this Assembly startupAssembly)
    {
      return startupAssembly?.Location
        .Replace(".dll", ".xml", StringComparison.InvariantCultureIgnoreCase)
        // This is here to work around an issue on Azure DevOps build agents not finding the .xml file.
        .Replace("$(BuildConfiguration)", "Debug", StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Creates an HTTP client for the specified test server.
    /// </summary>
    /// <param name="testServer">The test server.</param>
    /// <param name="testContext">The test context.</param>
    /// <returns>The HTTP client.</returns>
    public static HttpClient CreateClient(this TestServer testServer, TestContext testContext)
    {
      return new HttpClient(new HttpClientLoggingHandler(testContext, testServer.CreateHandler()))
      {
        BaseAddress = testServer.BaseAddress
      };
    }
  }
}
