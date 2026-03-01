using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class TaxaCambioConfiguration : IEntityTypeConfiguration<TaxaCambio>
{
    public void Configure(EntityTypeBuilder<TaxaCambio> builder)
    {
        builder.ToTable("TaxasCambio");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.MoedaOrigem)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.MoedaDestino)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(e => e.Taxa)
            .HasPrecision(18, 8);

        builder.Property(e => e.Fonte)
            .HasMaxLength(100);

        builder.HasIndex(e => new { e.MoedaOrigem, e.MoedaDestino, e.DataReferencia });
    }
}
