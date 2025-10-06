using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using triage_backend.Interfaces;
using triage_backend.Repositories;
using triage_backend.Services;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using triage_backend.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;


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

// Patient
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

// IA
builder.Services.AddHttpClient<IHuggingFaceService, HuggingFaceService>();


// ------------------- Controladores -------------------
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
