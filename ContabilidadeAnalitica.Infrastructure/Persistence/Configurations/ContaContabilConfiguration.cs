using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class ContaContabilConfiguration : IEntityTypeConfiguration<ContaContabil>
{
    public void Configure(EntityTypeBuilder<ContaContabil> builder)
    {
        builder.ToTable("ContasContabeis");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Codigo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Descricao)
            .HasMaxLength(500);

        builder.Property(e => e.Natureza)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Subtipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.ExpressaoCalculo)
            .HasMaxLength(1000);

        // Hierarquia auto-referenciada
        builder.HasOne(e => e.ContaPai)
            .WithMany(e => e.ContasFilhas)
            .HasForeignKey(e => e.ContaPaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Saldos)
            .WithOne(s => s.ContaContabil)
            .HasForeignKey(s => s.ContaContabilId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.PlanoContasId, e.Codigo }).IsUnique();
        builder.HasIndex(e => e.ContaPaiId);
        builder.HasIndex(e => e.Natureza);
    }
}
