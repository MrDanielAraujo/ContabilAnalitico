using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class SaldoContaConfiguration : IEntityTypeConfiguration<SaldoConta>
{
    public void Configure(EntityTypeBuilder<SaldoConta> builder)
    {
        builder.ToTable("SaldosContas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Ano).IsRequired();
        builder.Property(e => e.Mes).IsRequired();

        builder.Property(e => e.Valor)
            .HasPrecision(18, 4);

        builder.Property(e => e.SaldoDevedor)
            .HasPrecision(18, 4);

        builder.Property(e => e.SaldoCredor)
            .HasPrecision(18, 4);

        builder.Property(e => e.Moeda)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("BRL");

        builder.Property(e => e.Origem)
            .HasMaxLength(50);

        builder.HasIndex(e => new { e.ContaContabilId, e.EmpresaId, e.Ano, e.Mes }).IsUnique();
        builder.HasIndex(e => new { e.EmpresaId, e.Ano });
    }
}
