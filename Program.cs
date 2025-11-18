using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using triage_backend;
using triage_backend.Controllers;
using triage_backend.Interfaces;
using triage_backend.Repositories;
using triage_backend.Services;
using triage_backend.Utilities;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Configuración de CORS -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            // ⭐ Permitir tu dominio de producción en Vercel
            .WithOrigins(
                "https://triage-frontend.vercel.app",
                "https://triage-frontend-nexs.vercel.app/",
                "http://localhost:3000",
                "https://localhost:5173",
                "https://*.devtunnels.ms"
            )
            // ⭐ Permitir otros dominios y previews de Vercel
            .SetIsOriginAllowed(o =>
                o.Contains(".vercel.app") ||       // previews
                o.Contains("localhost") ||
                o.Contains("devtunnels.ms")
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
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
builder.Services.AddScoped<ITriageRepository, TriageRepository>();

// Triage Result Nurse (Confirmación de triaje por enfermero)
builder.Services.AddScoped<TriageResultRepository>();
builder.Services.AddScoped<ITriageResultService, TriageResultService>();

// Tratamiento
builder.Services.AddScoped<TreatmentRepository>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();


// Mostrar lista de pacientes al medico 
builder.Services.AddScoped<MedicListPRepository>();
builder.Services.AddScoped<IMedicListPService, MedicListPService>();

// Manejar el Médico asignado, estado y tiempos
builder.Services.AddScoped<ConsultationRepository>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();

// Historial de usuario
builder.Services.AddScoped<TriageFullInfoRepository>();
builder.Services.AddScoped<ITriageFullInfoService, TriageFullInfoService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<HistoryRepository>();

// Reporte
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();


//Envio de correos 
builder.Services.AddScoped<PriorityUpdateRepository>();
builder.Services.AddScoped<IPriorityUpdateService, PriorityUpdateService>();

// Registrar servicios personalizados
builder.Services.AddScoped<IReportService, ReportService>();

// Medicamentos
builder.Services.AddScoped<MedicationRepository>();
builder.Services.AddScoped<IMedicationService, MedicationService>();

// Exámenes
builder.Services.AddScoped<ExamRepository>();
builder.Services.AddScoped<IExamService, ExamService>();

// Diagnostico
builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();


// Dashboard
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<DashboardService>();

// Alertas de pacientes que notifican empeoramiento
builder.Services.AddScoped<AlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();

// Repositorio (si no lo registraste ya)
builder.Services.AddScoped<TriageBackend.Repositories.IHistoryRepository, TriageBackend.Repositories.HistoryReportRepository>();

// Servicio renombrado
builder.Services.AddScoped<TriageBackend.Services.IHistoryReportService, TriageBackend.Services.HistoryReportService>();

// PDF util
builder.Services.AddScoped<TriageBackend.Utilities.IPdfGeneratorHistoryReport, TriageBackend.Utilities.PdfGeneratorHistoryReport>();

// ▼ Configuración necesaria para QuestPDF
QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;



// ----------------------------------------------------------------------
// JWT
// ----------------------------------------------------------------------
// ------------------- JWT -------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
string jwtKey = jwtSection["Key"]!;
string jwtIssuer = jwtSection["Issuer"]!;
string jwtAudience = jwtSection["Audience"]!;

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

// 🔥 ESTO FALTABA
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})

.AddJwtBearer(options =>
{
    options.SaveToken = false;
    options.RequireHttpsMetadata = false; // <--- DEV y TUNNELS !!

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };

    // 👇 LEER TOKEN DESDE COOKIE HttpOnly
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            if (ctx.Request.Cookies.ContainsKey("X-Auth"))
            {
                ctx.Token = ctx.Request.Cookies["X-Auth"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
// ------------------- Swagger -------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Triage API - Intelligent Triage System ",
        Version = "v1",
        Description = @"API del sistema de triaje inteligente para apaoyar el sisttema de urgencias.  
        Permite la clasificación automática y validación manual de pacientes mediante IA y personal médico.  
        *Repositorio en GitHub:* [Ir al proyecto](https://github.com/LauraOrtiz11/triage_backend)",
        Contact = new OpenApiContact
        {
            Name = "Equipo de Desarrollo - Proyecto Triage",
            Url = new Uri("https://github.com/LauraOrtiz11/triage_backend")
        },

    });

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

    // Incluir comentarios XML para la documentación detallada
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// ------------------- Pipeline HTTP -------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Triage API v1");
        c.DocumentTitle = "Triage API Documentation";
        c.InjectStylesheet("/swagger-ui/custom.css");
    });
}

// Middleware de errores personalizados
app.UseMiddleware<triage_backend.Utilities.Middleware.ErrorHandlingMiddleware>();



// Bloquear rutas desconocidas y limpiar URL
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Response.Redirect("/"); // redirige a raíz (o login)
    }
});

app.UseCors("DevCors");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();


app.Run();