using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CertificadoConfiguration : IEntityTypeConfiguration<Certificado>
{
    public void Configure(EntityTypeBuilder<Certificado> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.NombreArchivo).IsRequired().HasMaxLength(255);
        builder.Property(c => c.RutaArchivo).IsRequired().HasMaxLength(500);
        builder.Property(c => c.Estado).IsRequired().HasMaxLength(20);

        builder.HasOne(c => c.Socio)
            .WithMany(s => s.Certificados)
            .HasForeignKey(c => c.IdSocio)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
