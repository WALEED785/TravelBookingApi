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
// 4) AutoMapper (if needed)
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
// 6) Elasticsearch Configuration (Simplified)
// ------------------------
var elasticsearchUrl = builder.Configuration["Elasticsearch:Url"] ?? "http://localhost:9200";
var requestTimeoutMinutes = builder.Configuration.GetValue<int>("Elasticsearch:RequestTimeoutMinutes", 2);

// Create Elasticsearch client with simple configuration
var elasticSettings = new ConnectionSettings(new Uri(elasticsearchUrl))
    .RequestTimeout(TimeSpan.FromMinutes(requestTimeoutMinutes))
    .ThrowExceptions(); // This will help with debugging

// Add Elasticsearch client as singleton (thread-safe)
builder.Services.AddSingleton<IElasticClient>(new ElasticClient(elasticSettings));

// Register Elasticsearch service as scoped
builder.Services.AddScoped<IElasticsearchService, ElasticsearchService>();

// ------------------------
// 7) Swagger
// ------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Travel Booking API with Elasticsearch",
        Version = "v1",
        Description = "Simple API for travel bookings with Elasticsearch search"
    });

    // Add XML comments if available
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

// ---------- Health checks ----------
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow }));

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

// Initialize Elasticsearch indices on startup
using (var scope = app.Services.CreateScope())
{
    var elasticService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Initializing Elasticsearch indices...");
        var result = await elasticService.CreateIndicesAsync();
        if (result)
        {
            logger.LogInformation("Elasticsearch indices initialized successfully");
        }
        else
        {
            logger.LogWarning("Failed to initialize Elasticsearch indices");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error initializing Elasticsearch indices on startup");
    }
}

app.Run();