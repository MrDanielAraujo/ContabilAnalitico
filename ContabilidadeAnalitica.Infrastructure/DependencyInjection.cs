using ContabilidadeAnalitica.Domain.Interfaces;
using ContabilidadeAnalitica.Infrastructure.Persistence;
using ContabilidadeAnalitica.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ContabilidadeAnalitica.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra serviços de infraestrutura (DbContext, repositórios, UoW).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString = null)
    {
        // Configurar DbContext
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // SQLite em memória para desenvolvimento
            services.AddDbContext<ContabilidadeDbContext>(options =>
                options.UseSqlite("DataSource=:memory:")
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors());
        }
        else if (connectionString.StartsWith("Host=", StringComparison.OrdinalIgnoreCase))
        {
            // PostgreSQL para produção
            services.AddDbContext<ContabilidadeDbContext>(options =>
                options.UseNpgsql(connectionString));
        }
        else
        {
            // SQLite em arquivo
            services.AddDbContext<ContabilidadeDbContext>(options =>
                options.UseSqlite(connectionString));
        }

        // Repositórios
        services.AddScoped<IGrupoEmpresarialRepository, GrupoEmpresarialRepository>();
        services.AddScoped<IEmpresaRepository, EmpresaRepository>();
        services.AddScoped<IPlanoContasRepository, PlanoContasRepository>();
        services.AddScoped<IContaContabilRepository, ContaContabilRepository>();
        services.AddScoped<ISaldoContaRepository, SaldoContaRepository>();
        services.AddScoped<ITemplateDemonstrativoRepository, TemplateDemonstrativoRepository>();
        services.AddScoped<IImportacaoRepository, ImportacaoRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
