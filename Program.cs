using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using triage_backend;
using triage_backend.Interfaces;
using triage_backend.Repositories;
using triage_backend.Services;
using triage_backend.Utilities;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ------------------- CORS -------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy
            .WithOrigins(
                "https://triage-frontend.vercel.app",
                "https://triage-frontend-nexs.vercel.app",
                "http://localhost:3000",
                "https://localhost:5173",
                "https://*.devtunnels.ms"
            )
            .SetIsOriginAllowed(o =>
                o.Contains(".vercel.app") ||
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

// ------------------- Servicios y Repositorios -------------------
builder.Services.AddScoped<ContextDB>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<AutenticationRepository>();
builder.Services.AddScoped<IAutenticationService, AutenticationService>();

builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddHttpClient<IHuggingFaceService, HuggingFaceService>();

builder.Services.AddSingleton<ITokenService, TokenService>();

builder.Services.AddScoped<IRevokedTokenRepository, RevokedTokenRepository>();

builder.Services.AddScoped<TriageDataService>();
builder.Services.AddScoped<HuggingFaceService>();

builder.Services.AddScoped<ITriagePatientService, TriageService>();
builder.Services.AddScoped<ITriageRepository, TriageRepository>();

builder.Services.AddScoped<TriageResultRepository>();
builder.Services.AddScoped<ITriageResultService, TriageResultService>();

builder.Services.AddScoped<TreatmentRepository>();
builder.Services.AddScoped<ITreatmentService, TreatmentService>();

builder.Services.AddScoped<MedicListPRepository>();
builder.Services.AddScoped<IMedicListPService, MedicListPService>();

builder.Services.AddScoped<ConsultationRepository>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();

builder.Services.AddScoped<TriageFullInfoRepository>();
builder.Services.AddScoped<ITriageFullInfoService, TriageFullInfoService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();
builder.Services.AddScoped<HistoryRepository>();

builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddScoped<PriorityUpdateRepository>();
builder.Services.AddScoped<IPriorityUpdateService, PriorityUpdateService>();

builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<MedicationRepository>();

builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<ExamRepository>();

builder.Services.AddScoped<IDiagnosisService, DiagnosisService>();

builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddScoped<AlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();

builder.Services.AddScoped<TriageBackend.Repositories.IHistoryRepository, TriageBackend.Repositories.HistoryReportRepository>();
builder.Services.AddScoped<TriageBackend.Services.IHistoryReportService, TriageBackend.Services.HistoryReportService>();
builder.Services.AddScoped<TriageBackend.Utilities.IPdfGeneratorHistoryReport, TriageBackend.Utilities.PdfGeneratorHistoryReport>();

QuestPDF.Settings.License = LicenseType.Community;
QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

// ------------------- JWT -------------------
var jwtSection = builder.Configuration.GetSection("Jwt");
string jwtKey = jwtSection["Key"]!;
string jwtIssuer = jwtSection["Issuer"]!;
string jwtAudience = jwtSection["Audience"]!;

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = false;
    options.RequireHttpsMetadata = false;

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
        Title = "Triage API - Intelligent Triage System",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        In = ParameterLocation.Header,
        Description = "Escribe: Bearer {token}"
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
            }, new string[] {}
        }
    });
});

var app = builder.Build();

// ------------------- Middleware -------------------
app.UseSwagger();
app.UseSwaggerUI();


app.UseMiddleware<triage_backend.Utilities.Middleware.ErrorHandlingMiddleware>();

// ------------------- CORS, HTTPS, Auth -------------------
app.UseCors("DevCors");

// Railway NO usa HTTPS internamente — deja activo SOLO si usas reverse-proxy
// app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// ------------------- Endpoints -------------------
app.MapControllers();

// ------------------- PUERTO DINÁMICO (✔ RAILWAY) -------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
