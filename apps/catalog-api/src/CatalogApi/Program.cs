using CatalogApi.Configuration;
using CatalogApi.Handlers;
using CatalogApi.Middleware;
using CatalogApi.Models.Requests;
using CatalogApi.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configuration
builder.Services.Configure<ApiConfiguration>(
    builder.Configuration.GetSection(ApiConfiguration.SectionName));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("KnowledgeBase");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    return client.GetDatabase("catalog_kb");
});

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis");
    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddScoped<IDatabase>(serviceProvider =>
{
    var connectionMultiplexer = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
    return connectionMultiplexer.GetDatabase();
});

// Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

// Application services
builder.Services.AddScoped<IKnowledgeBaseService, KnowledgeBaseService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IObservabilityService, ObservabilityService>();

// Health checks
builder.Services.AddHealthChecks()
    .AddMongoDb(builder.Configuration.GetConnectionString("KnowledgeBase")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Middleware pipeline
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<CachingMiddleware>();

app.UseCors("AllowAll");

// API endpoints
var api = app.MapGroup("/api/v1");

// Search endpoints
api.MapGet("/search", SearchHandler.Search)
    .WithName("Search")
    .WithOpenApi();

// Collection endpoints
api.MapGet("/collections/{name}", CollectionsHandler.GetCollectionByName)
    .WithName("GetCollection")
    .WithOpenApi();

// Type endpoints
api.MapGet("/types/{fqcn}", TypesHandler.GetTypeByFqcn)
    .WithName("GetType")
    .WithOpenApi();

// Graph endpoints
api.MapGet("/graph", GraphHandler.GetGraph)
    .WithName("GetGraph")
    .WithOpenApi();

// Diff endpoints
api.MapGet("/diff/type/{fqcn}", DiffHandler.GetTypeDiff)
    .WithName("GetTypeDiff")
    .WithOpenApi();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/mongodb", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("mongodb")
});
app.MapHealthChecks("/health/redis", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("redis")
});

app.Run();