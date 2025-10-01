using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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

// Patient
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

// IA
builder.Services.AddHttpClient<IHuggingFaceService, HuggingFaceService>();


// ------------------- Controladores -------------------
builder.Services.AddControllers();

// ------------------- Swagger -------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ------------------- Pipeline HTTP -------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
