using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ProductApi.API.Extensions;
using ProductApi.API.Filters;
using ProductApi.API.Middleware;
using ProductApi.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging (Serilog) ----
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// ---- Services ----
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiVersioningSupport();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddCorsPolicy(builder.Configuration);
builder.Services.AddResponseCompression();

var app = builder.Build();

// ---- Middleware pipeline ----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Product API v1");
    });
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseCors("DefaultCorsPolicy");
app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply pending EF Core migrations automatically on startup (useful for containerized envs).
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (app.Configuration.GetValue<bool>("ApplyMigrationsOnStartup"))
    {
        dbContext.Database.Migrate();
    }
}

app.Run();

// Exposed for WebApplicationFactory-based integration tests.
public partial class Program { }
