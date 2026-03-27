using ProductManagement.AppDbContext_EFCore;
using ProductManagement.GENERIC_REPOSITORY;
using ProductManagement.JWT_TOKEN_SERVICE;
using ProductManagement.Services;
using System.Text;

namespace ProductManagement.SERVICE
{
    public static class ServiceExtensions
    {
        // ── Database ─────────────────────────────
        public static IServiceCollection AddDatabase(
            this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                    npgsql => npgsql.MigrationsAssembly("AspNetMonolith")));

            return services;
        }

        // ── JWT Authentication ────────────────────
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, IConfiguration config)
        {
            var secret = config["JwtSettings:Secret"]!;
            var key = Encoding.UTF8.GetBytes(secret);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = config["JwtSettings:Issuer"],
                        ValidAudience = config["JwtSettings:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ClockSkew = TimeSpan.Zero    // no grace period
                    };

                    // Return proper JSON on 401/403 instead of empty response
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = 401;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(
                                """{"success":false,"message":"Unauthorized. Please login.","statusCode":401}""");
                        },
                        OnForbidden = async context =>
                        {
                            context.Response.StatusCode = 403;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(
                                """{"success":false,"message":"Forbidden. Insufficient permissions.","statusCode":403}""");
                        }
                    };
                });

            // ─── FUTURE OAUTH2 HOOK ──────────────────────
            // To add Google OAuth, simply chain here:
            //
            // .AddGoogle(options =>
            // {
            //     options.ClientId     = config["OAuth:Google:ClientId"]!;
            //     options.ClientSecret = config["OAuth:Google:ClientSecret"]!;
            // })
            //
            // Or for a full OIDC provider:
            // .AddOpenIdConnect("provider-name", options => { ... })
            // ─────────────────────────────────────────────

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                    policy.RequireRole("Admin"));

                options.AddPolicy("UserOrAdmin", policy =>
                    policy.RequireRole("User", "Admin"));
            });

            return services;
        }

        // ── Repositories (Scoped = one per HTTP request) ──
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            // Add new repos here as your app grows
            return services;
        }

        // ── Services ─────────────────────────────
        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            // Add new services here
            return services;
        }

        // ── Swagger with JWT support ──────────────
        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "AspNet Monolith API",
                    Version = "v1",
                    Description = "Industry-grade ASP.NET Core + PostgreSQL + JWT"
                });

                // Add "Authorize" button in Swagger UI
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter: Bearer {your_token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            });

            return services;
        }
    }
}
