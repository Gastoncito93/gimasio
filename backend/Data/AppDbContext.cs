using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Plan> Planes => Set<Plan>();
    public DbSet<Actividad> Actividades => Set<Actividad>();
    public DbSet<Socio> Socios => Set<Socio>();
    public DbSet<Cuota> Cuotas => Set<Cuota>();
    public DbSet<Certificado> Certificados => Set<Certificado>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<SocioProgreso> SociosProgresos => Set<SocioProgreso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
