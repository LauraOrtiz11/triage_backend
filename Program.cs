using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using triage_backend.Interfaces;
using triage_backend.Repositories;
using triage_backend.Services;
using triage_backend.Utilities;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000") // URL de tu frontend
            .AllowAnyHeader()
            .AllowAnyMethod()
    );
});

builder.Services.AddControllers();



// ------------------- Servicios -------------------
// Contexto de BD
builder.Services.AddScoped<ContextDB>();

// User
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Auth
builder.Services.AddScoped<AutenticationRepository>();
builder.Services.AddScoped<IAutenticationService, AutenticationService>();

// Patient
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

// IA
builder.Services.AddHttpClient<IHuggingFaceService, HuggingFaceService>();

// Token service (asegúrate de tener ITokenService y TokenService implementados)
builder.Services.AddSingleton<ITokenService, TokenService>();

// tokens revocados
builder.Services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();

// Triage Patient   
builder.Services.AddScoped<ITriagePatientService, TriageService>();

// ------------------- Configuración CORS (dev) -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ------------------- Configuración JWT (Authentication + Authorization) -------------------
// Lee claves/issuer/audience desde appsettings (asegúrate de tenerlas allí)
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not set in configuration");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "triage_backend";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "triage_backend_users";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// Registra esquema por defecto
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // en dev se puede dejar false; en prod true
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero,

            // IMPORTANT: indica cuál claim contiene el role
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role


        };
        // OnTokenValidated: validar jti en tabla RevokedTokens
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                try
                {
                    var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    if (string.IsNullOrEmpty(jti))
                    {
                        // Si no hay jti, fallamos la validación
                        context.Fail("Token missing jti");
                        return;
                    }

                    // Servicio para comprobar tokens revocados (tu repo)
                    var revokedRepo = context.HttpContext.RequestServices.GetService<IRevokedTokenRepository>();
                    if (revokedRepo == null)
                    {
                        // si no está registrado, opcionalmente permitir o fallar. Aquí solo salimos.
                        return;
                    }

                    var isRevoked = await revokedRepo.IsRevokedAsync(jti);
                    if (isRevoked)
                    {
                        context.Fail("Token revoked");
                        return;
                    }

                    // opcional: podrías comprobar que el usuario sigue activo en BD, etc.
                }
                catch (Exception ex)
                {
                    // Log si lo necesitas
                    context.Fail("OnTokenValidated error: " + ex.Message);
                }
            }
        };
    });

builder.Services.AddAuthorization();

// ------------------- Controladores / Swagger -------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Triage API", Version = "v1" });

    // Configurar JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce: Bearer {tu token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var app = builder.Build();

// ------------------- Pipeline HTTP -------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Cors debe ir antes de UseAuthentication/UseAuthorization si tu UI lo necesita
app.UseCors("DevCors");

app.UseHttpsRedirection();

// Importante: primero autenticación, luego autorización
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
