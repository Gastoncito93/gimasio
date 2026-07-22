using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Descripcion).HasMaxLength(255);
        builder.Property(p => p.PrecioMensual).HasPrecision(10, 2);
        builder.Property(p => p.Estado).IsRequired().HasMaxLength(20);

        builder.HasMany(p => p.Socios)
            .WithOne(s => s.Plan)
            .HasForeignKey(s => s.IdPlan)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
