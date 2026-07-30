using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(AppDbContext context)
    {
        try
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE Cuotas DROP INDEX IX_Cuotas_IdSocio_Periodo;");
        }
        catch
        {
            // Index already dropped or not present
        }

        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE SociosProgresos ADD COLUMN TipoRegistro VARCHAR(255) NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE SociosProgresos ADD COLUMN EjercicioNombre VARCHAR(255) NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE SociosProgresos ADD COLUMN ValorMetrica DECIMAL(18,2) NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE SociosProgresos ADD COLUMN UnidadMetrica VARCHAR(255) NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Usuarios ADD COLUMN DebeCambiarPassword TINYINT(1) NOT NULL DEFAULT 1;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Usuarios ADD COLUMN TokenRecuperacion VARCHAR(255) NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Usuarios ADD COLUMN TokenRecuperacionExpiracion DATETIME NULL;"); } catch { }
        try { await context.Database.ExecuteSqlRawAsync("ALTER TABLE Socios MODIFY COLUMN IdPlan INT NULL;"); } catch { }

        // 1. Seed / Normalizar Roles
        var rolAdmin = await context.Roles.FirstOrDefaultAsync(r => r.Id == 1);
        if (rolAdmin == null)
        {
            context.Roles.Add(new Rol { Id = 1, Nombre = "Administrador" });
        }
        else if (rolAdmin.Nombre != "Administrador")
        {
            rolAdmin.Nombre = "Administrador";
        }

        var rolCoach = await context.Roles.FirstOrDefaultAsync(r => r.Id == 2);
        if (rolCoach == null)
        {
            context.Roles.Add(new Rol { Id = 2, Nombre = "Coach" });
        }
        else if (rolCoach.Nombre != "Coach")
        {
            rolCoach.Nombre = "Coach";
        }

        var rolAlumno = await context.Roles.FirstOrDefaultAsync(r => r.Id == 3);
        if (rolAlumno == null)
        {
            context.Roles.Add(new Rol { Id = 3, Nombre = "Alumno" });
        }

        await context.SaveChangesAsync();

        // 2. Seed Actividades iniciales
        if (!await context.Actividades.AnyAsync())
        {
            context.Actividades.AddRange(
                new Actividad { Id = 1, Nombre = "Musculación", Descripcion = "Entrenamiento de fuerza y sala libre", Estado = "Activo" },
                new Actividad { Id = 2, Nombre = "Crossfit", Descripcion = "Entrenamiento funcional de alta intensidad", Estado = "Activo" },
                new Actividad { Id = 3, Nombre = "Spinning", Descripcion = "Ciclismo de interior guiado", Estado = "Activo" },
                new Actividad { Id = 4, Nombre = "Yoga", Descripcion = "Flexibilidad, postura y relajación", Estado = "Activo" }
            );
            await context.SaveChangesAsync();
        }

        // 3. Seed Plan inicial si no existe ninguno
        if (!await context.Planes.AnyAsync())
        {
            context.Planes.Add(new Plan
            {
                Nombre = "Musculación & Pase Libre",
                PrecioMensual = 15000,
                Descripcion = "Acceso libre a la sala de musculación y clases grupales.",
                Estado = "Activo"
            });
            await context.SaveChangesAsync();
        }

        var planInicial = await context.Planes.FirstAsync(p => p.Estado == "Activo");

        // 4. Seed / Normalizar Usuarios

        // Admin
        if (!await context.Usuarios.AnyAsync(u => u.Username == "admin"))
        {
            var adminUser = new Usuario
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Nombre = "Administrador Principal",
                IdRol = 1
            };
            context.Usuarios.Add(adminUser);
        }

        // Coach Principal (Musculación)
        var empleadoExistente = await context.Usuarios.FirstOrDefaultAsync(u => u.Username == "empleado");
        if (empleadoExistente != null)
        {
            empleadoExistente.Username = "coach";
            empleadoExistente.Nombre = "Coach Principal";
            empleadoExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Coach123!");
            empleadoExistente.IdRol = 2;
            empleadoExistente.IdActividad = 1; // Musculación
        }
        else if (!await context.Usuarios.AnyAsync(u => u.Username == "coach"))
        {
            var coachUser = new Usuario
            {
                Username = "coach",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Coach123!"),
                Nombre = "Coach Principal",
                IdRol = 2,
                IdActividad = 1 // Musculación
            };
            context.Usuarios.Add(coachUser);
        }
        else
        {
            var existingCoach = await context.Usuarios.FirstAsync(u => u.Username == "coach");
            if (existingCoach.IdActividad == null)
            {
                existingCoach.IdActividad = 1;
            }
        }

        // Coach 2 (Roberto Gómez - Musculación)
        if (!await context.Usuarios.AnyAsync(u => u.Username == "coach_roberto"))
        {
            context.Usuarios.Add(new Usuario
            {
                Username = "coach_roberto",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Coach123!"),
                Nombre = "Coach Roberto Gómez",
                IdRol = 2,
                IdActividad = 1 // Musculación
            });
        }

        // Coach 3 (Elena Páez - Crossfit)
        if (!await context.Usuarios.AnyAsync(u => u.Username == "coach_elena"))
        {
            context.Usuarios.Add(new Usuario
            {
                Username = "coach_elena",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Coach123!"),
                Nombre = "Coach Elena Páez",
                IdRol = 2,
                IdActividad = 2 // Crossfit
            });
        }

        // Alumno
        if (!await context.Usuarios.AnyAsync(u => u.Username == "alumno"))
        {
            var alumnoUser = new Usuario
            {
                Username = "alumno",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Alumno123!"),
                Nombre = "Alumno de Prueba",
                IdRol = 3
            };
            context.Usuarios.Add(alumnoUser);
        }

        await context.SaveChangesAsync();

        // 5. Vincular Socio (Alumno de prueba) con el Coach de prueba
        var usuarioAlumno = await context.Usuarios.FirstOrDefaultAsync(u => u.Username == "alumno");
        var usuarioCoach = await context.Usuarios.FirstOrDefaultAsync(u => u.Username == "coach");

        if (usuarioAlumno != null && usuarioCoach != null)
        {
            var socioAlumno = await context.Socios.FirstOrDefaultAsync(s => s.IdUsuario == usuarioAlumno.Id);
            if (socioAlumno == null)
            {
                socioAlumno = await context.Socios.FirstOrDefaultAsync(s => s.Dni == "40123456");
                if (socioAlumno == null)
                {
                    socioAlumno = new Socio
                    {
                        Dni = "40123456",
                        NombreCompleto = "Alumno de Prueba",
                        Telefono = "11-4455-6677",
                        Email = "alumno@gimnasio.com",
                        Estado = "Activo",
                        Observacion = "Alumno asignado al Coach principal.",
                        IdPlan = planInicial.Id,
                        IdUsuario = usuarioAlumno.Id,
                        IdCoach = usuarioCoach.Id
                    };
                    context.Socios.Add(socioAlumno);
                }
                else
                {
                    socioAlumno.IdUsuario = usuarioAlumno.Id;
                    socioAlumno.IdCoach = usuarioCoach.Id;
                }
                await context.SaveChangesAsync();
            }
        }

        // 6. Seed de Alumnos Realistas en la Base de Datos
        if (usuarioCoach != null)
        {
            var alumnosRealistas = new List<Socio>
            {
                new Socio
                {
                    Dni = "38452109",
                    NombreCompleto = "Carlos Bustamante",
                    Telefono = "11-5421-9876",
                    Email = "carlos.bustamante@gmail.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-5),
                    Observacion = "Objetivo: Hipertrofia y acondicionamiento general.",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                },
                new Socio
                {
                    Dni = "41209384",
                    NombreCompleto = "Mariana Rossi",
                    Telefono = "11-6123-4567",
                    Email = "mariana.rossi@outlook.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-3),
                    Observacion = "Entrenamiento funcional e hidratación adecuada.",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                },
                new Socio
                {
                    Dni = "39871234",
                    NombreCompleto = "Gonzalo Fernández",
                    Telefono = "11-4987-1234",
                    Email = "g.fernandez@yahoo.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-4),
                    Observacion = "Preparación para maratón de 10k.",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                },
                new Socio
                {
                    Dni = "42109876",
                    NombreCompleto = "Sofía Villalba",
                    Telefono = "11-3456-7890",
                    Email = "sofia.villalba@gmail.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-2),
                    Observacion = "Rutina enfocada en fuerza de miembros inferiores.",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                },
                new Socio
                {
                    Dni = "40567890",
                    NombreCompleto = "Mateo Benítez",
                    Telefono = "11-2345-6789",
                    Email = "mateo.benitez@hotmail.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-6),
                    Observacion = "Rehabilitación de hombro derecho finalizada.",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                },
                new Socio
                {
                    Dni = "43890123",
                    NombreCompleto = "Lucía Domínguez",
                    Telefono = "11-7890-1234",
                    Email = "lucia.dominguez@gmail.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-1),
                    Observacion = "Acondicionamiento físico y tonificación.",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                },
                new Socio
                {
                    Dni = "37654321",
                    NombreCompleto = "Nicolás Cabrera",
                    Telefono = "11-8901-2345",
                    Email = "nico.cabrera@outlook.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-7),
                    Observacion = "Crossfit y levantamiento olímpico.",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                },
                new Socio
                {
                    Dni = "44123456",
                    NombreCompleto = "Valentina Morales",
                    Telefono = "11-9012-3456",
                    Email = "valen.morales@gmail.com",
                    Estado = "Activo",
                    FechaAlta = DateTime.UtcNow.AddMonths(-2),
                    Observacion = "Entrenamiento de alta intensidad (HIIT).",
                    IdPlan = planInicial.Id,
                    IdCoach = usuarioCoach.Id
                }
            };

            foreach (var nuevoSocio in alumnosRealistas)
            {
                if (!await context.Socios.AnyAsync(s => s.Dni == nuevoSocio.Dni))
                {
                    context.Socios.Add(nuevoSocio);
                }
            }

            await context.SaveChangesAsync();
        }

        // 7. Seed de Cuotas del Mes para Socios Activos
        var activeSocios = await context.Socios.Include(s => s.Plan).Where(s => s.Estado == "Activo").ToListAsync();
        var currentPeriodo = int.Parse(DateTime.UtcNow.ToString("yyyyMM"));
        var currentMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        int countSeeded = 0;
        foreach (var s in activeSocios)
        {
            var planPrice = s.Plan?.PrecioMensual ?? 15000m;
            if (planPrice <= 0) planPrice = 15000m;

            if (!await context.Cuotas.AnyAsync(c => c.IdSocio == s.Id && c.Periodo == currentPeriodo))
            {
                context.Cuotas.Add(new Cuota
                {
                    IdSocio = s.Id,
                    Periodo = currentPeriodo,
                    Monto = planPrice,
                    FechaVencimiento = currentMonthStart.AddDays(9),
                    FechaPago = DateTime.UtcNow.AddDays(-countSeeded),
                    Estado = "Pagada",
                    Observacion = "Cuota abonada en término"
                });
                countSeeded++;
            }
        }
        await context.SaveChangesAsync();
    }
}
