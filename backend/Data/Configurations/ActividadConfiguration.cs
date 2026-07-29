using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

public class ActividadConfiguration : IEntityTypeConfiguration<Actividad>
{
    public void Configure(EntityTypeBuilder<Actividad> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Nombre).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Descripcion).HasMaxLength(500);
        builder.Property(a => a.Estado).IsRequired().HasMaxLength(20).HasDefaultValue("Activo");

        builder.HasData(
            new Actividad { Id = 1, Nombre = "Musculación", Descripcion = "Entrenamiento de fuerza y sala libre", Estado = "Activo" },
            new Actividad { Id = 2, Nombre = "Crossfit", Descripcion = "Entrenamiento funcional de alta intensidad", Estado = "Activo" },
            new Actividad { Id = 3, Nombre = "Spinning", Descripcion = "Ciclismo de interior guiado", Estado = "Activo" },
            new Actividad { Id = 4, Nombre = "Yoga", Descripcion = "Flexibilidad, postura y relajación", Estado = "Activo" }
        );
    }
}
