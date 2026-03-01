using System.Text.Json.Serialization;
using ContabilidadeAnalitica.Application.UseCases.Demonstrativos;
using ContabilidadeAnalitica.Application.UseCases.Empresas;
using ContabilidadeAnalitica.Application.UseCases.Importacao;
using ContabilidadeAnalitica.Application.UseCases.PlanoContas;
using ContabilidadeAnalitica.Application.UseCases.Templates;
using ContabilidadeAnalitica.CalculationEngine.Core;
using ContabilidadeAnalitica.Infrastructure;
using ContabilidadeAnalitica.Infrastructure.Data;
using ContabilidadeAnalitica.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── JSON Serialization ──
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// ── Swagger ──
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Contabilidade Analítica API",
        Version = "v1",
        Description = "Sistema contábil analítico de nível institucional para cálculo de demonstrativos financeiros (Balanço e Balancete)."
    });

    var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
    foreach (var xmlFile in xmlFiles)
    {
        c.IncludeXmlComments(xmlFile);
    }
});

// ── Infrastructure (DbContext com SQLite em memória persistente) ──
var keepAliveConnection = new SqliteConnection("DataSource=ContabilidadeAnalitica;Mode=Memory;Cache=Shared");
keepAliveConnection.Open();

builder.Services.AddDbContext<ContabilidadeDbContext>(options =>
    options.UseSqlite(keepAliveConnection)
        .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
        .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// ── Repositories & UoW ──
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.IGrupoEmpresarialRepository,
    ContabilidadeAnalitica.Infrastructure.Persistence.Repositories.GrupoEmpresarialRepository>();
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.IEmpresaRepository,
    ContabilidadeAnalitica.Infrastructure.Persistence.Repositories.EmpresaRepository>();
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.IPlanoContasRepository,
    ContabilidadeAnalitica.Infrastructure.Persistence.Repositories.PlanoContasRepository>();
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.IContaContabilRepository,
    ContabilidadeAnalitica.Infrastructure.Persistence.Repositories.ContaContabilRepository>();
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.ISaldoContaRepository,
    ContabilidadeAnalitica.Infrastructure.Persistence.Repositories.SaldoContaRepository>();
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.ITemplateDemonstrativoRepository,
    ContabilidadeAnalitica.Infrastructure.Persistence.Repositories.TemplateDemonstrativoRepository>();
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.IImportacaoRepository,
    ContabilidadeAnalitica.Infrastructure.Persistence.Repositories.ImportacaoRepository>();
builder.Services.AddScoped<ContabilidadeAnalitica.Domain.Interfaces.IUnitOfWork,
    ContabilidadeAnalitica.Infrastructure.Persistence.UnitOfWork>();

// ── CalculationEngine ──
builder.Services.AddScoped<IMotorCalculoContabil, MotorCalculoContabil>();

// ── Application Use Cases ──
builder.Services.AddScoped<EmpresaUseCases>();
builder.Services.AddScoped<PlanoContasUseCases>();
builder.Services.AddScoped<ContaContabilUseCases>();
builder.Services.AddScoped<TemplateUseCases>();
builder.Services.AddScoped<ImportacaoUseCases>();
builder.Services.AddScoped<DemonstrativoUseCases>();

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ── Database Initialization & Seed ──
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ContabilidadeDbContext>();
    await context.Database.EnsureCreatedAsync();
    await SeedData.InicializarAsync(context);
}

// ── Middleware Pipeline ──
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contabilidade Analítica API v1");
    c.RoutePrefix = string.Empty;
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
