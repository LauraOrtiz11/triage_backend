using triage_backend.Repositories;
using triage_backend.Services;
using triage_backend.Utilities;

var builder = WebApplication.CreateBuilder(args);

// ------------------- Servicios -------------------
// Contexto de BD
builder.Services.AddScoped<ContextDB>();

// User
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// Patient
builder.Services.AddScoped<PatientRepository>();
builder.Services.AddScoped<IPatientService, PatientService>();

// ------------------- Controladores -------------------
builder.Services.AddControllers();

<<<<<<< HEAD
//  Swagger 
builder.Services.AddEndpointsApiExplorer();0
=======
// ------------------- Swagger -------------------
builder.Services.AddEndpointsApiExplorer();
>>>>>>> origin/QA
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

app.MapControllers();

app.Run();
