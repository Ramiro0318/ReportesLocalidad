using Microsoft.EntityFrameworkCore;
using ReportesLocalidadApi.Mappings;
using ReportesLocalidadApi.Models;
using ReportesLocalidadApi.Models.Entities;
using ReportesLocalidadApi.Repositories;
using ReportesLocalidadApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("No se encontro la cadena de conexion DefaultConnection.");
}

builder.Services.AddControllers();
builder.Services.AddDbContext<ReportesLocalidadContext>(options => options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(config =>
{
    config.AddProfile<MappingProfile>();
});
builder.Services.AddDbContext<ReportesLocalidadContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped(typeof(Repository<>));
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<ReporteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();
