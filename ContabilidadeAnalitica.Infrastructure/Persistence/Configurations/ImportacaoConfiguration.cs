using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class ImportacaoConfiguration : IEntityTypeConfiguration<Importacao>
{
    public void Configure(EntityTypeBuilder<Importacao> builder)
    {
        builder.ToTable("Importacoes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Descricao)
            .HasMaxLength(500);

        builder.Property(e => e.SistemaOrigem)
            .HasMaxLength(100);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.MensagemErro)
            .HasMaxLength(2000);

        builder.HasMany(e => e.Itens)
            .WithOne(i => i.Importacao)
            .HasForeignKey(i => i.ImportacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.EmpresaId);
        builder.HasIndex(e => e.Status);
    }
}

public class ImportacaoItemConfiguration : IEntityTypeConfiguration<ImportacaoItem>
{
    public void Configure(EntityTypeBuilder<ImportacaoItem> builder)
    {
        builder.ToTable("ImportacaoItens");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CodigoConta)
            .IsRequired()
            .HasMaxLength(50);

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

        builder.Property(e => e.MensagemErro)
            .HasMaxLength(1000);

        builder.HasIndex(e => e.ImportacaoId);
    }
}
