using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PortalFornecedores.Api.Data;
using PortalFornecedores.Api.Services;
using PortalFornecedores.Api.Validation;

var builder = WebApplication.CreateBuilder(args);

// Enums viram texto no JSON ("Aceita" em vez de 0)
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Banco PostgreSQL; colunas em snake_case (chave_acesso) para facilitar consultas SQL
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
     .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<LeitorXmlNfe>();
builder.Services.AddScoped<ValidadorNotaFiscal>();

// Permite o frontend (localhost:3000) chamar a API
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors();
app.MapControllers();

app.Run();
