using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class GrupoEmpresarialConfiguration : IEntityTypeConfiguration<GrupoEmpresarial>
{
    public void Configure(EntityTypeBuilder<GrupoEmpresarial> builder)
    {
        builder.ToTable("GruposEmpresariais");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Nome)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Descricao)
            .HasMaxLength(500);

        builder.Property(e => e.CodigoExterno)
            .HasMaxLength(50);

        builder.HasMany(e => e.Empresas)
            .WithOne(e => e.GrupoEmpresarial)
            .HasForeignKey(e => e.GrupoEmpresarialId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.Nome);
    }
}
