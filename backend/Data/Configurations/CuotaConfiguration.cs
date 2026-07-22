using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class CuotaConfiguration : IEntityTypeConfiguration<Cuota>
{
    public void Configure(EntityTypeBuilder<Cuota> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Periodo).IsRequired();
        builder.Property(c => c.Monto).HasPrecision(10, 2);
        builder.Property(c => c.Estado).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Observacion).HasMaxLength(255);

        builder.HasIndex(c => new { c.IdSocio, c.Periodo }).IsUnique();

        builder.HasOne(c => c.Socio)
            .WithMany(s => s.Cuotas)
            .HasForeignKey(c => c.IdSocio)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
