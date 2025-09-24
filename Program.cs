using triage_backend.Repositories;
using triage_backend.Services;
using triage_backend.Utilities;

var builder = WebApplication.CreateBuilder(args);

//  servicios 
builder.Services.AddScoped<ContextDB>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

// controladores
builder.Services.AddControllers();

//  Swagger 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//  Configuraci?n del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
