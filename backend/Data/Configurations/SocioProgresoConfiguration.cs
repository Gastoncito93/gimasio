using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class SocioProgresoConfiguration : IEntityTypeConfiguration<SocioProgreso>
{
    public void Configure(EntityTypeBuilder<SocioProgreso> builder)
    {
        builder.ToTable("SociosProgresos");

        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Fecha)
            .IsRequired();

        builder.Property(sp => sp.PesoKg)
            .HasPrecision(5, 2);

        builder.Property(sp => sp.Observaciones)
            .HasMaxLength(500);

        builder.Property(sp => sp.RutaFotoFrente)
            .HasMaxLength(255);

        builder.Property(sp => sp.RutaFotoPerfil)
            .HasMaxLength(255);

        builder.Property(sp => sp.RutaFotoEspalda)
            .HasMaxLength(255);

        builder.HasOne(sp => sp.Socio)
            .WithMany(s => s.Progresos)
            .HasForeignKey(sp => sp.IdSocio)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
