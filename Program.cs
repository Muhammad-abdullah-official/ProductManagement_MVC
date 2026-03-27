// ─────────────────────────────────────────────
//  PROGRAM.CS  —  App entry point
//
//  Two phases:
//  1. Builder phase  → register services (DI container)
//  2. Pipeline phase → configure middleware order
//
//  Middleware ORDER matters:
//  Exception → HTTPS → CORS → Auth → Authorization → Controllers
// ─────────────────────────────────────────────

// ── Serilog setup (before anything else) ─────
using Microsoft.AspNetCore.RateLimiting;
using ProductManagement.AppDbContext_EFCore;
using ProductManagement.MIDDLEWARE;
using ProductManagement.SERVICE;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog replace default logging ──────
    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .WriteTo.Console()
              .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day));

    // ═══════════════════════════════════════════
    //  SERVICE REGISTRATIONS  (DI Container)
    // ═══════════════════════════════════════════
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddRepositories();
    builder.Services.AddAppServices();
    builder.Services.AddSwaggerWithJwt();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // CORS — allow frontend dev server
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevPolicy", policy =>
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials());

        // Tighter production policy
        options.AddPolicy("ProdPolicy", policy =>
            policy.WithOrigins("https://yourdomain.com")
                  .WithHeaders("Authorization", "Content-Type")
                  .WithMethods("GET", "POST", "PUT", "DELETE"));
    });

    // Rate limiting (built-in .NET 8)
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("api", limiterOptions =>
        {
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.PermitLimit = 100;
            limiterOptions.QueueLimit = 0;
        });
    });

    // ═══════════════════════════════════════════
    //  APP / MIDDLEWARE PIPELINE
    // ═══════════════════════════════════════════
    var app = builder.Build();

    // 1. Global exception handler — must be FIRST
    app.UseMiddleware<ExceptionMiddleware>();

    // 2. Swagger (dev only)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AspNet Monolith v1");
            c.RoutePrefix = string.Empty;   // serve at root URL
        });
    }

    // 3. HTTPS redirect
    app.UseHttpsRedirection();

    // 4. Serilog request logging
    app.UseSerilogRequestLogging();

    // 5. CORS
    app.UseCors(app.Environment.IsDevelopment() ? "DevPolicy" : "ProdPolicy");

    // 6. Rate limiter
    app.UseRateLimiter();

    // 7. Authentication (Who are you?)
    app.UseAuthentication();

    // 8. Authorization (What can you do?)
    app.UseAuthorization();

    // 9. Controllers
    app.MapControllers();

    // ── Auto-run DB migrations on startup ────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Database migration complete.");
    }

    Log.Information("Application starting on {Env}", app.Environment.EnvironmentName);
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start.");
}
finally
{
    Log.CloseAndFlush();
}