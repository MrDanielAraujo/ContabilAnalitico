using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContabilidadeAnalitica.Infrastructure.Persistence;

/// <summary>
/// DbContext principal do sistema contábil.
/// Configurado para SQLite em memória (dev) com preparação para PostgreSQL (produção).
/// </summary>
public class ContabilidadeDbContext : DbContext
{
    public ContabilidadeDbContext(DbContextOptions<ContabilidadeDbContext> options)
        : base(options) { }

    public DbSet<GrupoEmpresarial> GruposEmpresariais => Set<GrupoEmpresarial>();
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<PlanoContas> PlanosContas => Set<PlanoContas>();
    public DbSet<ContaContabil> ContasContabeis => Set<ContaContabil>();
    public DbSet<SaldoConta> SaldosContas => Set<SaldoConta>();
    public DbSet<TemplateDemonstrativo> TemplatesDemonstrativos => Set<TemplateDemonstrativo>();
    public DbSet<TemplateLinha> TemplateLinhas => Set<TemplateLinha>();
    public DbSet<Importacao> Importacoes => Set<Importacao>();
    public DbSet<ImportacaoItem> ImportacaoItens => Set<ImportacaoItem>();
    public DbSet<TaxaCambio> TaxasCambio => Set<TaxaCambio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas as configurações do assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContabilidadeDbContext).Assembly);
    }

    /// <summary>
    /// Override para preencher campos de auditoria automaticamente.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.EntidadeBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CriadoEm = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.AlteradoEm = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
