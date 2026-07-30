using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class SocioConfiguration : IEntityTypeConfiguration<Socio>
{
    public void Configure(EntityTypeBuilder<Socio> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Dni).IsRequired().HasMaxLength(20);
        builder.HasIndex(s => s.Dni).IsUnique();
        builder.Property(s => s.NombreCompleto).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Telefono).HasMaxLength(30);
        builder.Property(s => s.Email).HasMaxLength(100);
        builder.Property(s => s.Estado).IsRequired().HasMaxLength(20);
        builder.Property(s => s.Observacion).HasMaxLength(500);

        builder.HasOne(s => s.Plan)
            .WithMany(p => p.Socios)
            .HasForeignKey(s => s.IdPlan)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Usuario)
            .WithOne()
            .HasForeignKey<Socio>(s => s.IdUsuario)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(s => s.Coach)
            .WithMany(u => u.AlumnosComoCoach)
            .HasForeignKey(s => s.IdCoach)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
