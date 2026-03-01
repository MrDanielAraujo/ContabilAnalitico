using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class TemplateDemonstrativoConfiguration : IEntityTypeConfiguration<TemplateDemonstrativo>
{
    public void Configure(EntityTypeBuilder<TemplateDemonstrativo> builder)
    {
        builder.ToTable("TemplatesDemonstrativos");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Descricao)
            .HasMaxLength(500);

        builder.Property(e => e.Tipo)
            .IsRequired()
            .HasConversion<int>();

        builder.HasMany(e => e.Linhas)
            .WithOne(l => l.TemplateDemonstrativo)
            .HasForeignKey(l => l.TemplateDemonstrativoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.Tipo);
    }
}

public class TemplateLinhaConfiguration : IEntityTypeConfiguration<TemplateLinha>
{
    public void Configure(EntityTypeBuilder<TemplateLinha> builder)
    {
        builder.ToTable("TemplateLinhas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Rotulo)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.TipoLinha)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.CodigoConta)
            .HasMaxLength(50);

        builder.Property(e => e.NaturezaFiltro)
            .HasConversion<int?>();

        builder.Property(e => e.SubtipoFiltro)
            .HasConversion<int?>();

        builder.Property(e => e.Expressao)
            .HasMaxLength(1000);

        builder.Property(e => e.LinhasReferenciadas)
            .HasMaxLength(2000);

        builder.HasIndex(e => new { e.TemplateDemonstrativoId, e.Ordem });
    }
}
