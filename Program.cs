using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Nest;
using TravelBookingApi.Data;
using TravelBookingApi.Repositories.Implementations;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.AISearch;
using TravelBookingApi.Services.Implementations;
using TravelBookingApi.Services.Interfaces;
using TravelBookingApi.Utilities;

var builder = WebApplication.CreateBuilder(args);

// ------------------------
// 1) Controllers
// ------------------------
builder.Services.AddControllers();

// ------------------------
// 2) EF Core / SQL Server
// ------------------------
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ------------------------
// 3) CORS (dev)
// ------------------------
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("ReactAppPolicy", p =>
    {
        p.WithOrigins(
            "http://localhost:3200",
            "http://localhost:3000"
        )
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

// ------------------------
// 4) AutoMapper
// ------------------------
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ------------------------
// 5) Repositories & Services
// ------------------------
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDestinationRepository, DestinationRepository>();

builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ITokenService, SimpleTokenService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// ------------------------
// 6) Elasticsearch Configuration
// ------------------------
var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
var defaultIndex = builder.Configuration["Elasticsearch:DefaultIndex"] ?? "travel_bookings";
var enableDebugMode = builder.Configuration.GetValue<bool>("Elasticsearch:EnableDebugMode", true);
var requestTimeoutMinutes = builder.Configuration.GetValue<int>("Elasticsearch:RequestTimeoutMinutes", 2);

var elasticSettings = new ConnectionSettings(new Uri(elasticsearchUrl))
    .DefaultIndex(defaultIndex)
    .RequestTimeout(TimeSpan.FromMinutes(requestTimeoutMinutes));

if (enableDebugMode)
{
    elasticSettings.EnableDebugMode().PrettyJson();
}

// Add as singleton since ElasticClient is thread-safe
builder.Services.AddSingleton<IElasticClient>(new ElasticClient(elasticSettings));

// Register Elasticsearch service as scoped (better for controllers)
builder.Services.AddSingleton<IElasticsearchService, ElasticsearchService>();

// Register the initializer as a hosted service
builder.Services.AddHostedService<ElasticsearchInitializerService>();

// ------------------------
// 7) Swagger
// ------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Travel Booking API",
        Version = "v1",
        Description = "API for managing travel bookings, hotels, and flights with Elasticsearch integration"
    });

    // Add XML comments if you have them
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ------------------------
// 8) HTTP Pipeline
// ------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Booking API v1");
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });

    app.UseCors("ReactAppPolicy");
}
else
{
    app.UseCors(p => p
        .WithOrigins("https://your-production-domain.com")
        .AllowAnyHeader()
        .AllowAnyMethod());
}

// ---------- DB migrations ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if ((await db.Database.GetPendingMigrationsAsync()).Any())
        await db.Database.MigrateAsync();
}

app.UseHttpsRedirection();
app.UseRouting();

// ---------- Global error handler ----------
app.UseExceptionHandler(handler =>
{
    handler.Run(async ctx =>
    {
        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.ContentType = "application/json";

        var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (ex is null) return;

        var code = ex switch
        {
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        ctx.Response.StatusCode = code;
        await ctx.Response.WriteAsJsonAsync(new
        {
            StatusCode = code,
            Message = ex.Message,
            Details = app.Environment.IsDevelopment() ? ex.StackTrace : null
        });
    });
});

// ---------- Endpoints ----------
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

// ---------- Elasticsearch health check ----------
app.MapGet("/health/elasticsearch", async (IElasticClient elasticClient) =>
{
    try
    {
        var response = await elasticClient.PingAsync();
        return Results.Ok(new
        {
            Status = response.IsValid ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            ElasticsearchUrl = elasticsearchUrl
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(new()
        {
            Title = "Elasticsearch Health Check Failed",
            Detail = ex.Message,
            Status = 503
        });
    }
});

app.Run();