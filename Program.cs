using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TravelBookingApi.Data;
using TravelBookingApi.Repositories.Implementations;
using TravelBookingApi.Repositories.Interfaces;
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
        p.WithOrigins("http://localhost:3200")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials();
    });
});

// ------------------------
// 4) AutoMapper – NEW
// ------------------------
// Scans *all* loaded assemblies, so you do NOT have to list Profiles one?by?one.
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
builder.Services.AddScoped<IBookingService, BookingService>();   // our hand?mapped service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDestinationService, DestinationService>();
builder.Services.AddScoped<ITokenService, SimpleTokenService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();

// ------------------------
// 6) Swagger
// ------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Travel Booking API",
        Version = "v1",
        Description = "API for managing travel bookings, hotels, and flights"
    });
});

var app = builder.Build();

// ------------------------
// 7) HTTP Pipeline
// ------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Travel Booking API v1");
        c.DisplayRequestDuration();
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
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.Run();
