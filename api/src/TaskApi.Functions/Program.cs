using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskApi.Functions.Data;
using TaskApi.Functions.Repositories;
using TaskApi.Functions.Factories;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Hosting;
using TaskApi.Functions.Services;
using TaskApi.Functions.Middleware;
using Microsoft.Azure.Functions.Worker.Middleware;

var builder = FunctionsApplication.CreateBuilder(args);

// Configura Application Insights (telemetría opcional)
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Configuración de EF Core + DI
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    var conn = builder.Configuration.GetValue<string>("SqlServer:ConnectionString")
               ?? throw new InvalidOperationException("SqlServer:ConnectionString is required");

    opt.UseSqlServer(conn, sql => sql.EnableRetryOnFailure());
});

// Repositories y Factories
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<ITaskFactory, TaskApi.Functions.Factories.TaskFactory>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IUserFactory, UserFactory>();
builder.Services.AddScoped<IUserService, UserService>();

// JWT Auth middleware (use pipeline registration so it actually runs)
builder.UseMiddleware<JwtAuthenticationMiddleware>();

// Configuración JSON global para enums
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
});
// Construir y correr
builder.Build().Run();
