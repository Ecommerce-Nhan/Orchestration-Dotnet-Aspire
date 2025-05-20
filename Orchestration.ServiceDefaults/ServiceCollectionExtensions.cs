using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Debugging;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

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

    public static IServiceCollection AddJWTConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("IxrAjDoa2FqElO7IhrSrUJELhUckePEPVpaePlS_Xaw"));
            o.RequireHttpsMetadata = false;
            o.SaveToken = true;
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                //ValidIssuer = "https://localhost:5001/",
                //ValidAudience = "b865bfc2-9966-4309-93be-f0dcd2d7c59b",
                IssuerSigningKey = key,
            };
            o.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    return context.Response.WriteAsync("{\"error\":\"Unauthorized\"}");
                }
            };
        });
        services.AddAuthorization();

        return services;
    }
}