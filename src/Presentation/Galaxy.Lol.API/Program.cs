using System.Text;
using Asp.Versioning;
using Galaxy.Lol.API.Filters;
using Galaxy.Lol.API.Middlewares;
using Galaxy.Lol.API.Security;
using Galaxy.Lol.Application;
using Galaxy.Lol.Infraestructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var otlpEndpoint = builder.Configuration["OpenTelemetry:OtlpEndpoint"]
    ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
    ?? "http://jaeger_lol_hex:4317";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: "galaxy-lol-hexagonal-api",
        serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint)));

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddControllers(options => options.Filters.Add<UnitOfWorkFilter>());

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.Configure<JwtSettings>(options =>
{
    builder.Configuration.GetSection(JwtSettings.SectionName).Bind(options);

    var secretoEntorno = builder.Configuration["JWT_SECRET"]
                         ?? Environment.GetEnvironmentVariable("JWT_SECRET");

    if (!string.IsNullOrWhiteSpace(secretoEntorno))
        options.SecretKey = secretoEntorno;
});
builder.Services.AddScoped<JwtTokenGenerator>();

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
var claveFirma = builder.Configuration["JWT_SECRET"]
                 ?? Environment.GetEnvironmentVariable("JWT_SECRET")
                 ?? jwt.SecretKey;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(string.IsNullOrWhiteSpace(claveFirma)
                    ? Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")
                    : claveFirma))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Galaxy LoL Champions - Arquitectura Hexagonal",
        Version = "v1",
        Description = "Explorador y analitica de campeones de League of Legends " +
                      "(Data Dragon + Riot Games API) implementado con puertos y adaptadores."
    });

    var esquema = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Obtenga el token en POST /api/v1/auth/token y peguelo aqui.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    options.AddSecurityDefinition("Bearer", esquema);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquema] = [] });
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Galaxy LoL Champions v1");

    options.RoutePrefix = string.Empty;
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", timestamp = DateTime.UtcNow }))
   .AllowAnonymous()
   .ExcludeFromDescription();

app.Run();
