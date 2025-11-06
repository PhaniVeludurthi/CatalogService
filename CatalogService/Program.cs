using CatalogService.Context;
using CatalogService.Repository;
using CatalogService.Repository.Interfaces;
using CatalogService.Services;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Catalog Service API",
        Version = "v1",
        Description = "API for managing events and venues"
    });
});

// Database
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IVenueRepository, VenueRepository>();
builder.Services.AddScoped<DatabaseSeeder>();

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

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowAll");

// Prometheus metrics endpoint
app.UseMetricServer();
app.UseHttpMetrics();

app.MapControllers();

// Health check endpoints
app.MapGet("/health/live", () => Results.Ok(new { status = "Healthy", service = "catalog-service" }))
   .WithName("Liveness")
   .WithTags("Health");

app.MapGet("/health/ready", async (CatalogDbContext dbContext) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync();

        if (canConnect)
        {
            return Results.Ok(new
            {
                status = "Ready",
                database = "Connected"
            });
        }

        return Results.StatusCode(503); // Service Unavailable
    }
    catch
    {
        return Results.StatusCode(503);
    }
})
.WithName("Readiness")
.WithTags("Health");


app.Run();
