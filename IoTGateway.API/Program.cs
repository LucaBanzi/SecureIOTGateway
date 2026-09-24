using IoTGateway.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Registra DbContext con PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Ora funzionerà grazie al pacchetto installato

var app = builder.Build();

// Abilitiamo Swagger sempre durante lo sviluppo locale
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IoT Gateway API v1");
    c.RoutePrefix = "swagger"; // Imposta Swagger sulla rotta /swagger
});

app.UseAuthorization();
app.MapControllers();

app.Run();