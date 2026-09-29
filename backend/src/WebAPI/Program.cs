using TicketWorld.Application;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Infrastructure;
using TicketWorld.WebAPI.Hubs;
using TicketWorld.WebAPI.Middlewares;
using TicketWorld.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Clean Architecture layer dependencies
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add SignalR real-time messaging
builder.Services.AddSignalR();
builder.Services.AddSingleton<ISeatMapNotifier, SignalRSeatMapNotifier>();

// Add WebAPI controllers and OpenAPI
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// 1. Correlation ID tracking
app.UseMiddleware<CorrelationIdMiddleware>();

// 2. Global RFC 7807 Exception Handler
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// 3. Virtual Waiting Room Concurrency Throttling
app.UseMiddleware<WaitingRoomMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Auto-seed development master data
    try
    {
        using var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database seeding was skipped or encountered an error during startup.");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SeatMapHub>("/hubs/seatmap");

app.Run();

// Partial declaration for integration test fixture visibility
public partial class Program { }
