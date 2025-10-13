using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using triage_backend.Interfaces;
using triage_backend.Repositories;
using triage_backend.Services;
using triage_backend.Utilities;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Configuración de CORS -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ------------------- Controladores -------------------
builder.Services.AddControllers();

// ------------------- Servicios -------------------
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

// IA (HuggingFace)
builder.Services.AddHttpClient<IHuggingFaceService, HuggingFaceService>();

// Token service
builder.Services.AddSingleton<ITokenService, TokenService>();

// Tokens revocados
builder.Services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();

// Triage 
builder.Services.AddScoped<TriageDataService>();
builder.Services.AddScoped<HuggingFaceService>();

// Triage Patient
builder.Services.AddScoped<ITriagePatientService, TriageService>();

// Triage Result Nurse (Confirmación de triage por enfermero)
builder.Services.AddScoped<TriageResultRepository>();
builder.Services.AddScoped<ITriageResultService, TriageResultService>();

// ------------------- Repositorios -------------------
builder.Services.AddScoped<ITriageRepository, TriageRepository>();


// ------------------- Configuración JWT -------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not set in configuration");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "triage_backend";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "triage_backend_users";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // en dev puede ser false
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
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                try
                {
                    var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                    if (string.IsNullOrEmpty(jti))
                    {
                        context.Fail("Token missing jti");
                        return;
                    }

                    var revokedRepo = context.HttpContext.RequestServices.GetService<IRevokedTokenRepository>();
                    if (revokedRepo == null) return;

                    var isRevoked = await revokedRepo.IsRevokedAsync(jti);
                    if (isRevoked) context.Fail("Token revoked");
                }
                catch (Exception ex)
                {
                    context.Fail("OnTokenValidated error: " + ex.Message);
                }
            }
        };
    });

builder.Services.AddAuthorization();

// ------------------- Swagger -------------------
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

app.UseCors("DevCors");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
