using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Debugging;

namespace Orchestration.ServiceDefaults;

public static class GeneralServiceExtensions
{
    public static void ConfigureSerilog(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
                                                       .CreateLogger();

        SelfLog.Enable(msg => Log.Information(msg));
        Log.Information("Starting server.");
    }

    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            var redisHost = configuration.GetConnectionString("redis");
            options.Configuration = redisHost;
        });

        return services;
    }

    public static IServiceCollection AddHangfireConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddHangfire(x => x
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_110)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UsePostgreSqlStorage(a =>
                                              a.UseNpgsqlConnection(connectionString),
                                              new PostgreSqlStorageOptions
                                              {
                                                  QueuePollInterval = TimeSpan.FromSeconds(30),
                                                  UseNativeDatabaseTransactions = false,
                                                  DistributedLockTimeout = TimeSpan.FromMinutes(10),
                                                  InvisibilityTimeout = TimeSpan.FromMinutes(10),
                                              })
                        );
        services.AddHangfireServer();

        return services;
    }
}