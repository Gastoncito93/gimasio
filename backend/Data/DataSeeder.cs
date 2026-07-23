using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(AppDbContext context)
    {
        // Seed Admin user
        if (!await context.Usuarios.AnyAsync(u => u.Username == "admin"))
        {
            var adminUser = new Usuario
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Nombre = "Administrador",
                IdRol = 1
            };
            context.Usuarios.Add(adminUser);
        }

        // Seed Empleado user
        if (!await context.Usuarios.AnyAsync(u => u.Username == "empleado"))
        {
            var empleadoUser = new Usuario
            {
                Username = "empleado",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Empleado123!"),
                Nombre = "Empleado",
                IdRol = 2
            };
            context.Usuarios.Add(empleadoUser);
        }

        await context.SaveChangesAsync();
    }
}
