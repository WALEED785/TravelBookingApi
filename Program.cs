using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TravelBookingApi.Data;
using TravelBookingApi.Repositories.Implementations;
using TravelBookingApi.Repositories.Interfaces;
using TravelBookingApi.Services.Implementations;
using TravelBookingApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure DbContext with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register repositories
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Register services
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, SimpleTokenService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Travel Booking API",
        Version = "v1",
        Description = "API for managing travel bookings, hotels, and flights"
    });

    // Add Token Authentication to Swagger
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter your token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    };

    c.AddSecurityDefinition("TokenAuth", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "TokenAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Booking API v1");
        c.DisplayRequestDuration();
    });
}

// Apply database migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Check if there are pending migrations
    if ((await dbContext.Database.GetPendingMigrationsAsync()).Any())
    {
        // Apply migrations if there are any pending
        await dbContext.Database.MigrateAsync();
    }
}

app.UseHttpsRedirection();

// Custom token-based authentication middleware
app.Use(async (context, next) =>
{
    var path = context.Request.Path;
    if (!path.StartsWithSegments("/api/user/login") &&
        !path.StartsWithSegments("/api/user/register") &&
        !path.StartsWithSegments("/swagger") &&
        !path.StartsWithSegments("/health"))
    {
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authorization token is required");
            return;
        }

        var token = authHeader.ToString().Replace("Bearer ", "");
        var tokenService = context.RequestServices.GetRequiredService<ITokenService>();
        var userRepo = context.RequestServices.GetRequiredService<IUserRepository>();

        var user = await userRepo.GetByTokenAsync(token);
        if (user == null || user.TokenExpiry < DateTime.UtcNow)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid or expired token");
            return;
        }

        // Store user in context for later use if needed
        context.Items["User"] = user;
    }

    await next();
});

app.UseRouting();

// Enable CORS if needed
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());
// Add this before app.MapControllers()
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<IExceptionHandlerFeature>();
        if (error != null)
        {
            var ex = error.Error;
            var statusCode = ex switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                ArgumentException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = statusCode,
                Message = ex.Message
            });
        }
    });
});

app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.Run();