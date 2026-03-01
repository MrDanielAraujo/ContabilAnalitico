using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class PlanoContasConfiguration : IEntityTypeConfiguration<PlanoContas>
{
    public void Configure(EntityTypeBuilder<PlanoContas> builder)
    {
        builder.ToTable("PlanosContas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Descricao)
            .HasMaxLength(500);

        builder.Property(e => e.Versao)
            .IsRequired();

        builder.Property(e => e.VigenciaInicio)
            .IsRequired();

        builder.HasMany(e => e.Contas)
            .WithOne(c => c.PlanoContas)
            .HasForeignKey(c => c.PlanoContasId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.EmpresaId, e.Vigente });
        builder.HasIndex(e => new { e.EmpresaId, e.Versao }).IsUnique();
    }
}
