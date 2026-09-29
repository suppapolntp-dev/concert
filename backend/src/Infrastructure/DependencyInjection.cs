namespace TicketWorld.Infrastructure;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Repositories;
using TicketWorld.Infrastructure.Identity;
using TicketWorld.Infrastructure.Persistence;
using TicketWorld.Infrastructure.Persistence.Interceptors;
using TicketWorld.Infrastructure.Persistence.Repositories;
using TicketWorld.Infrastructure.Persistence.Seeding;
using TicketWorld.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        // Register SaveChanges Interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();

        // Register PostgreSQL DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=ticketworld_db;Username=ticketworld_admin;Password=ChangeMe_StrongPostgresPassword2026!";

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>());
        });

        // Register IApplicationDbContext contract
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Register Repositories and Unit of Work
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<ISeatRepository, SeatRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register Identity & Core Infrastructure Services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        services.AddScoped<IDataSeeder, ApplicationDbContextInitializer>();
        services.AddScoped<ApplicationDbContextInitializer>();
        services.AddScoped<SqlScriptSeeder>();

        // Register Redis ConnectionMultiplexer with resilient background connect
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? configuration["REDIS_CONNECTION_STRING"]
            ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            try
            {
                var configOptions = ConfigurationOptions.Parse(redisConnectionString);
                configOptions.AbortOnConnectFail = false;
                configOptions.ConnectTimeout = 3000;
                configOptions.SyncTimeout = 3000;
                return ConnectionMultiplexer.Connect(configOptions);
            }
            catch (Exception ex)
            {
                var logger = sp.GetService<ILogger<RedisDistributedLockService>>();
                logger?.LogWarning(ex, "Failed to initialize Redis connection multiplexer. Fallback lock will be used.");
                return null!;
            }
        });

        services.AddSingleton<IDistributedLockService, RedisDistributedLockService>();

        // Register JWT Authentication
        var jwtSecret = configuration["JWT_SECRET"]
            ?? configuration["Jwt:Secret"]
            ?? "super-secret-jwt-key-minimum-64-chars-long-ticketworld-production-ready-key-2026!";
        var jwtIssuer = configuration["JWT_ISSUER"] ?? configuration["Jwt:Issuer"] ?? "ticketworld-api";
        var jwtAudience = configuration["JWT_AUDIENCE"] ?? configuration["Jwt:Audience"] ?? "ticketworld-client";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
