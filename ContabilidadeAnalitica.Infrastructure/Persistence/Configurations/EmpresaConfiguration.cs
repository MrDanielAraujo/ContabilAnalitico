using ContabilidadeAnalitica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContabilidadeAnalitica.Infrastructure.Persistence.Configurations;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("Empresas");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RazaoSocial)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.NomeFantasia)
            .HasMaxLength(300);

        builder.Property(e => e.CNPJ)
            .HasMaxLength(18);

        builder.Property(e => e.CodigoExterno)
            .HasMaxLength(50);

        builder.Property(e => e.MoedaPadrao)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("BRL");

        builder.HasMany(e => e.PlanosContas)
            .WithOne(p => p.Empresa)
            .HasForeignKey(p => p.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Saldos)
            .WithOne(s => s.Empresa)
            .HasForeignKey(s => s.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Importacoes)
            .WithOne(i => i.Empresa)
            .HasForeignKey(i => i.EmpresaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CNPJ).IsUnique().HasFilter(null);
        builder.HasIndex(e => e.RazaoSocial);
    }
}
